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
            var where = new Dictionary<string, string[]> { { "id", new string[] { id } } };
            var data = mySQL.Read(Config.Tables.OWNERS, where);
            if (!(data.Rows.Count > 0))
            {
                throw new KeyNotFoundException(id);
            }
            Init(data.Rows[0], mySQL.Read(Config.Tables.OWNERSHIPS));
        }

        public Owner(DataRow row, DataTable ownerships) : base(row)
        {
            Init(row, ownerships);
        }

        private void Init(DataRow row, DataTable ownerships)
        {
            foreach (DataColumn column in row.Table.Columns)
            {
                _data[column.ToString()] = row[column] is DBNull ? null : row[column].ToString();
            }
            ownerships = ownerships.AsEnumerable()
                            .Where(row => row.Field<string>("owner_id") == _data["id"])
                            .CopyToDataTable();
            foreach (DataRow ownership in ownerships.Rows)
            {
                var merchant = ownership["merchant_id"] is DBNull ? null : ownership["merchant_id"].ToString();
                var percent = ownership["ownership"] is DBNull ? null : ownership["ownership"].ToString();
                if (merchant is null || percent is null)
                {
                    throw new NullReferenceException();
                }
                else
                {
                    _merchants[merchant] = double.Parse(percent);
                }
            }
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
            foreach (var pair in _data)
            {
                expando[pair.Key] = pair.Value;
            }
            expando["merchants"] = _merchants.ToDictionary(entry => new string(entry.Key), entry => entry.Value);
            return expando;
        }

        public static List<Owner> ReadAll(MySQL mySQL)
        {
            var owners = new List<Owner>();
            var ownerships = mySQL.Read(Config.Tables.OWNERSHIPS);
            foreach (DataRow row in mySQL.Read(Config.Tables.OWNERS).Rows)
            {
                owners.Add(new Owner(row, ownerships));
            }
            return owners;
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(GetJSON());
        }
    }
}
