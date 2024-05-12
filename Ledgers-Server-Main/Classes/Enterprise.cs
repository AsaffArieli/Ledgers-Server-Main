using System.Transactions;

namespace Ledgers_Server_Main.Classes
{
    public class Enterprise
    {
        private string _id;
        private MySQL _mySQL;

        private List<Funder> _funders = new List<Funder>();
        public List<Funder> Funders
        {
            get { return _funders; }
            private set { _funders = value; }
        }

        private List<Merchant> _merchants = new List<Merchant>();
        public List<Merchant> Merchants
        {
            get { return _merchants; }
            private set { _merchants = value; }
        }

        private List<Owner> _owners = new List<Owner>();
        public List<Owner> Owners
        {
            get { return _owners; }
            private set { _owners = value; }
        }

        public Enterprise()
        {
            _id = "1";
            _mySQL = new MySQL("database-1.czsmec4y8agx.us-east-2.rds.amazonaws.com", "admin", "Asaff2324", "Ledgers");
            _funders = Funder.ReadAll(_mySQL);
            _merchants = Merchant.ReadAll(_mySQL);
            _owners = Owner.ReadAll(_mySQL);
        }

        public void Insert(Dictionary<string, Dictionary<string, dynamic>[]> data)
        {
            var idDel = new Action<dynamic>(array =>
            {
                foreach (var item in array)
                    item["id"] = Guid.NewGuid().ToString().Replace("-", "");
            });
            using (TransactionScope scope = new TransactionScope())
            {
                if (data[Config.Tables.FUNDERS] is not null)
                {
                    idDel(data[Config.Tables.FUNDERS]);
                    _mySQL.Insert(Config.Tables.FUNDERS, data[Config.Tables.FUNDERS]);
                }
                if (data[Config.Tables.OWNERS] is not null)
                {
                    idDel(data[Config.Tables.OWNERS]);
                    _mySQL.Insert(Config.Tables.OWNERS, data[Config.Tables.OWNERS]);
                }
                if (data[Config.Tables.MERCHANTS] is not null)
                {
                    idDel(data[Config.Tables.MERCHANTS]);
                    _mySQL.Insert(Config.Tables.MERCHANTS, data[Config.Tables.MERCHANTS]);
                }
                if (data[Config.Tables.OWNERSHIPS] is not null)
                {
                    var ownerMap = new Dictionary<string, string>();
                    var merchantMap = new Dictionary<string, string>();
                    foreach (dynamic owner in data[Config.Tables.OWNERS])
                        ownerMap[owner.ssn] = owner.id;
                    foreach (dynamic merchant in data[Config.Tables.MERCHANTS])
                        merchantMap[merchant.ein] = merchant.id;
                    foreach (dynamic ownership in data[Config.Tables.OWNERSHIPS])
                    {
                        if (!ownerMap.TryGetValue(ownership.owner_id, out string ownerId))
                            throw new ArgumentException();
                        if (!merchantMap.TryGetValue(ownership.merchant_id, out string merchantId))
                            throw new ArgumentException();
                        ownership.owner_id = ownerId;
                        ownership.merchant_id = merchantId;
                    }
                    _mySQL.Insert(Config.Tables.OWNERSHIPS, data[Config.Tables.OWNERSHIPS]);
                }
                scope.Complete();
            }
        }

        public void Delete(string element, string id)
        {
            switch (element)
            {
                case Config.Tables.FUNDERS:
                    Funders.FirstOrDefault(item => item.Id.Equals(id))?.Delete(_mySQL);
                    break;
                case Config.Tables.MERCHANTS:
                    Merchants.FirstOrDefault(item => item.Id.Equals(id))?.Delete(_mySQL);
                    break;
                case Config.Tables.OWNERS:
                    Owners.FirstOrDefault(item => item.Id.Equals(id))?.Delete(_mySQL);
                    break;
                default:
                    throw new ArgumentException();
            }
        }
    }
}
