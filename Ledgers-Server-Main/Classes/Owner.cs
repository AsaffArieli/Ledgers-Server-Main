using System.Data;
using System.Dynamic;
using System.Text.Json;
using System.Transactions;

namespace Ledgers_Server_Main.Classes
{
    public class Owner : Element
    {
        private Dictionary<string, string?> _data = new Dictionary<string, string?>();
        private Dictionary<string, double> _merchants = new Dictionary<string, double>();

        public Owner(MySQL mySQL, string id) : base(id)
        {
            var data = Read(mySQL, id);
            if (data is null) { throw new KeyNotFoundException(id); }
            var where = new Dictionary<string, string[]> { { "owner_id", new string[] { id } } };
            Init(data, mySQL.Read(Config.Tables.OWNERSHIPS, where));
        }

        public Owner(DataRow row, DataTable ownerships) : base(row)
        {
            ownerships = ownerships.AsEnumerable().Where(row => row.Field<string>("owner_id") == Id).CopyToDataTable();
            Init(row, ownerships);
        }

        private void Init(DataRow row, DataTable ownerships)
        {
            row.Table.Columns.Cast<DataColumn>().ToList()
                .ForEach(column => _data[column.ToString()] = row[column]?.ToString());
            ownerships.AsEnumerable().ToList()
                .ForEach(ownership =>
                {
                    var merchant = ownership["merchant_id"] is DBNull ? null : ownership["merchant_id"].ToString();
                    var percent = ownership["ownership"] is DBNull ? null : ownership["ownership"].ToString();
                    if (merchant is null || percent is null) { throw new NullReferenceException(); }
                    else { _merchants[merchant] = double.Parse(percent); }
                });
        }

        public void Delete(MySQL mySQL)
        {
            using (TransactionScope scope = new TransactionScope())
            {
                mySQL.Delete(Config.Tables.OWNERS, Id);
                mySQL.Delete(Config.Tables.OWNERSHIPS, Id, "owner_id");
                scope.Complete();
            }
        }

        public dynamic GetJSON()
        {
            var expando = new ExpandoObject() as IDictionary<string, object?>;
            _data.ToList().ForEach(pair => expando[pair.Key] = pair.Value);
            expando["merchants"] = _merchants.ToDictionary(entry => new string(entry.Key), entry => entry.Value);
            return expando;
        }

        public static List<Owner> ReadAll(MySQL mySQL)
        {
            var ownerships = mySQL.Read(Config.Tables.OWNERSHIPS);
            return mySQL.Read(Config.Tables.OWNERS).AsEnumerable()
                .Select<DataRow, Owner>(owner => new Owner(owner, ownerships))
                .ToList();
        }

        private static DataRow? Read(MySQL mySQL, string id)
        {
            var where = new Dictionary<string, string[]> { { "id", new string[] { id } } };
            try { return mySQL.Read(Config.Tables.OWNERS, where).Rows[0]; }
            catch (IndexOutOfRangeException) { return null; }
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(GetJSON());
        }
    }
}
