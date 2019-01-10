using System;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using DnsClient;

public static class Utils
{
    public static Task<bool> IsValidAsync(string email)
    {
        try
        {
            var mailAddress = new MailAddress(email);
            var host = mailAddress.Host;
            return CheckDnsEntriesAsync(host);
        }

        catch (FormatException)
        {
            return Task.FromResult(false);
        }
    }

    public static async Task<bool> CheckDnsEntriesAsync(string domain)
    {
        try
        {
            var lookup = new LookupClient();
            lookup.Timeout = TimeSpan.FromSeconds(5);
            var result = await lookup.QueryAsync(domain, QueryType.ANY).ConfigureAwait(false);

            var records = result.Answers.Where(record => record.RecordType == DnsClient.Protocol.ResourceRecordType.A ||
                                                        record.RecordType == DnsClient.Protocol.ResourceRecordType.AAAA ||
                                                        record.RecordType == DnsClient.Protocol.ResourceRecordType.MX);
            return records.Any();
        }

        catch (DnsResponseException)
        {
            return false;
        }
    }

    public class Page<T> {
        public int Index { get; set; }
        public T[] Items { get; set; }
        public int TotalPages { get; set; }
    }

    public static Page<T> GetPage<T>(this Microsoft.EntityFrameworkCore.DbSet<T> List, int page_index, int page_size, Func<T, object> order_by_selector)
        where T : class
        {
            var res = List.OrderBy(order_by_selector)
                        .Skip(page_index * page_size)
                        .Take(page_size)
                        .ToArray();

            if(res == null || res.Length == 0)
                return null;
            
            var tot_items = List.Count();
            var tot_pages = tot_items / page_size;
            if(tot_items < page_size) tot_pages = 1;

            return new Page<T>(){Index = page_index, Items = res, TotalPages = tot_pages};
        }
}