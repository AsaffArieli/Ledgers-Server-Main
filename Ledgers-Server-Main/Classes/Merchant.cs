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
            var where = new Dictionary<string, string[]> { { "id", new string[] { id } } };
            var data = mySQL.Read(Config.Tables.MERCHANTS, where);
            if (!(data.Rows.Count > 0))
            {
                throw new KeyNotFoundException(id);
            }
            Init(data.Rows[0], mySQL.Read(Config.Tables.OWNERSHIPS));
        }

        public Merchant(DataRow row, DataTable ownerships) : base(row)
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
                            .Where(row => row.Field<string>("merchant_id") == Id)
                            .CopyToDataTable();
            foreach (DataRow ownership in ownerships.Rows)
            {
                var owner = ownership["owner_id"] is DBNull ? null : ownership["owner_id"].ToString();
                var percent = ownership["ownership"] is DBNull ? null : ownership["ownership"].ToString();
                if (owner is null || percent is null)
                {
                    throw new NullReferenceException();
                }
                else
                {
                    _owners[owner] = double.Parse(percent);
                }
            }
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
            foreach (var pair in _data)
            {
                expando[new string(pair.Key)] = new string(pair.Value);
            }
            expando["owners"] = _owners.ToDictionary(entry => new string(entry.Key), entry => entry.Value);
            return expando;
        }

        public static List<Merchant> ReadAll(MySQL mySQL)
        {
            var merchants = new List<Merchant>();
            var ownerships = mySQL.Read(Config.Tables.OWNERSHIPS);
            foreach (DataRow merchant in mySQL.Read(Config.Tables.MERCHANTS).Rows)
            {
                merchants.Add(new Merchant(merchant, ownerships));
            }
            return merchants;
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(GetJSON());
        }
    }
}
