using System.Data;
using System.Text.Json;
using System.Transactions;

namespace Ledgers_Server_Main.Classes
{
    public class Funder : Element
    {
        private Dictionary<string, string?> _data = new Dictionary<string, string?>();

        public Funder(MySQL mySQL, string id) : base(id)
        {
            var data = Read(mySQL, id);
            if (data is null) { throw new KeyNotFoundException(id); }
            Init(data);
        }

        public Funder(DataRow row) : base(row)
        {
            Init(row);
        }

        private void Init(DataRow row)
        {
            row.Table.Columns.Cast<DataColumn>().ToList()
                .ForEach(column => _data[column.ToString()] = row[column]?.ToString());
        }

        public void Delete(MySQL mySQL)
        {
            using (TransactionScope scope = new TransactionScope())
            {
                mySQL.Delete(Config.Tables.FUNDERS, Id);
                scope.Complete();
            }
        }

        public dynamic GetJSON()
        {
            return _data;
        }

        public static List<Funder> ReadAll(MySQL mySQL)
        {
            return mySQL.Read(Config.Tables.FUNDERS).AsEnumerable()
                .Select<DataRow, Funder>(funder => new Funder(funder))
                .ToList();
        }

        private static DataRow? Read(MySQL mySQL, string id)
        {
            var where = new Dictionary<string, string[]> { { "id", new string[] { id } } };
            try { return mySQL.Read(Config.Tables.FUNDERS, where).Rows[0]; }
            catch (IndexOutOfRangeException) { return null; }
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(_data);
        }
    }
}
