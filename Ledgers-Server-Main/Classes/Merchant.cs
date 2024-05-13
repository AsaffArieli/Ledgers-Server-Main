using Mysqlx.Crud;
using System.Data;
using System.Dynamic;
using System.Text.Json;
using System.Transactions;

namespace Ledgers_Server_Main.Classes
{
    public class Merchant : Element
    {
        private Dictionary<string, string?> _data = new Dictionary<string, string?>();
        private Dictionary<string, double> _owners = new Dictionary<string, double>();

        public Merchant(MySQL mySQL, string id) : base(id)
        {
            var data = Read(mySQL, id);
            if (data is null) { throw new KeyNotFoundException(id); }
            var where = new Dictionary<string, string[]> { { "merchant_id", new string[] { id } } };
            Init(data, mySQL.Read(Config.Tables.OWNERSHIPS, where));
        }

        public Merchant(DataRow row, DataTable ownerships) : base(row)
        {
            ownerships = ownerships.AsEnumerable().Where(row => row.Field<string>("merchant_id") == Id).CopyToDataTable();
            Init(row, ownerships);
        }

        private void Init(DataRow row, DataTable ownerships)
        {
            row.Table.Columns.Cast<DataColumn>().ToList()
                .ForEach(column => _data[column.ToString()] = row[column]?.ToString());
            ownerships.AsEnumerable().ToList()
                .ForEach(ownership =>
                {
                    var owner = ownership["owner_id"] is DBNull ? null : ownership["owner_id"].ToString();
                    var percent = ownership["ownership"] is DBNull ? null : ownership["ownership"].ToString();
                    if (owner is null || percent is null) { throw new NullReferenceException(); }
                    else { _owners[owner] = double.Parse(percent); }
                });
        }

        public void Delete(MySQL mySQL)
        {
            using (TransactionScope scope = new TransactionScope())
            {
                mySQL.Delete(Config.Tables.MERCHANTS, Id);
                mySQL.Delete(Config.Tables.OWNERSHIPS, Id, "merchant_id");
                scope.Complete();
            }
        }

        public dynamic GetJSON()
        {
            var expando = new ExpandoObject() as IDictionary<string, object?>;
            _data.ToList().ForEach(pair => expando[pair.Key] = pair.Value);
            expando["owners"] = _owners.ToDictionary(entry => new string(entry.Key), entry => entry.Value);
            return expando;
        }

        public static List<Merchant> ReadAll(MySQL mySQL)
        {
            var ownerships = mySQL.Read(Config.Tables.OWNERSHIPS);
            return mySQL.Read(Config.Tables.MERCHANTS).AsEnumerable()
                .Select<DataRow, Merchant>(merchant => new Merchant(merchant, ownerships))
                .ToList();
        }

        private static DataRow? Read(MySQL mySQL, string id)
        {
            var where = new Dictionary<string, string[]> { { "id", new string[] { id } } };
            try { return mySQL.Read(Config.Tables.MERCHANTS, where).Rows[0]; }
            catch (IndexOutOfRangeException) { return null; }
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(GetJSON());
        }
    }
}
