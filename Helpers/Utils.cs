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
}