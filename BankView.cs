using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Linq;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Banking
{
	class BankView : INotifyPropertyChanged
	{
		public static DataBankingDataContext db = new DataBankingDataContext();
		private string address;
		public string Address { get => address; set { address = value; OnPropertyChanged(""); } }

		private string nameBranch;
		public string NameBranch { get => nameBranch; set { nameBranch = value; OnPropertyChanged(""); } }

		private int select;
        public int Select 
		{
			get => select;
			set
			{ 
				select = value;
				Address = db.BRANCH.Where(x => x.BRANCH_ID == Select).Select(y => y.ADDRESS).FirstOrDefault();
				NameBranch = db.BRANCH.Where(x => x.BRANCH_ID == Select).Select(y => y.NAME).FirstOrDefault();
				OnPropertyChanged("");
			}
		}
		public List<string> Branch { get; set; }
        public BankView()
        {
			
			Branch = db.BRANCH.Select(x => x.CITY).ToList();
		}
		
		public event PropertyChangedEventHandler PropertyChanged;
		public void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

	}
}
