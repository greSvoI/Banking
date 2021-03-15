using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Linq;
using System.Data.Linq;
using System.Diagnostics;

namespace Banking
{
	/// <summary>
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{

		BankView bank;
		public MainWindow()
		{
			InitializeComponent();

			DataContext = bank = new BankView();

            comboBoxBranch.SelectionChanged += ComboBoxBranch_SelectionChanged;

            tabControl.SelectionChanged += TabControl_SelectionChanged;

			comboBoxClientOptions.SelectionChanged += (s, e) => { buttonAddProduct.Content = "Добавить"; buttonAddProduct.IsEnabled = true; };

			listBoxProductName.SelectionChanged += (s, e) => { buttonAddProduct.IsEnabled = true; };
			comboBoxEmployee.SelectionChanged += (s, e) => { tabControl.IsEnabled = true; };

			listBoxUser.SelectionChanged += (s, e) => 
			{ 
				stackPanelBottom.IsEnabled = true;
				comboBoxClientOptions.SelectedIndex = -1;
				buttonAddProduct.Content = "Онулировать";
			};

			tabControl.IsEnabled = false;
		}
		
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
			if(comboBoxEmployee.SelectedIndex !=-1)
            if (tabControl.SelectedIndex == 0)
            {
				stackPanelBottom.IsEnabled = false;
				listBoxUser.ItemsSource = BankView.db.EMPLOYEE.Where(x => x.ASSIGNED_BRANCH_ID == bank.Select);
            }
            else if (tabControl.SelectedIndex == 1)
            {
				//listBoxUser.ItemsSource = BankView.db.CUSTOMER.Where(x => x.CUST_TYPE_CD == "I" && x.CITY == comboBoxBranch.SelectedItem.ToString()); //Клиенты из этого филиала
				listBoxUser.ItemsSource = BankView.db.CUSTOMER.Where(x => x.CUST_TYPE_CD == "I");
                comboBoxClientOptions.ItemsSource = BankView.db.PRODUCT.Select(x => x.NAME).Where(y => y.Contains("business") != true);
            }
            else if (tabControl.SelectedIndex == 2)//Вип
            {
				//listBoxUser.ItemsSource = BankView.db.CUSTOMER.Where(x => x.CUST_TYPE_CD == "B");
				listBoxUser.ItemsSource = BankView.db.OFFICER;
                comboBoxClientOptions.ItemsSource = BankView.db.PRODUCT.Select(x => x.NAME);
            }
            else if (tabControl.SelectedIndex == 3)
            {
				listBoxUser.ItemsSource = BankView.db.CUSTOMER.Select(x => x.ACCOUNT.Select(y => y.ACC_TRANSACTION.Select(z => z.TELLER_EMP_ID)));
            }

        }

       

        private void buttonAddProduct_Click(object sender, RoutedEventArgs e)
        {
			if (textBoxCost.Text.All(x => !char.IsDigit(x)) && comboBoxClientOptions.SelectedIndex!=-1 && comboBoxEmployee.SelectedIndex != -1)
			{ 
				MessageBox.Show("Сумма введена не верно!"); return;
			}
			int cust_id=-1;
			CUSTOMER cust = new CUSTOMER();
			OFFICER offi = new OFFICER();

			if (listBoxUser.SelectedItem is CUSTOMER)
			{ 
				cust = (CUSTOMER)listBoxUser.SelectedItem;
				cust_id = cust.CUST_ID;
			}
			if (listBoxUser.SelectedItem is OFFICER)
			{
				offi = (OFFICER)listBoxUser.SelectedItem;

				cust_id =(int)BankView.db.OFFICER.Where(x => x.OFFICER_ID == offi.OFFICER_ID).Select(y => y.CUST_ID).FirstOrDefault(); 
			}

			if(cust_id!=-1)
            {
				ACCOUNT acc = new ACCOUNT();
				if (buttonAddProduct.Content.ToString() == "Добавить")
				{
					acc.AVAIL_BALANCE = double.Parse(textBoxCost.Text);
					acc.OPEN_BRANCH_ID = bank.Select;
					acc.OPEN_DATE = DateTime.Now;
					acc.PENDING_BALANCE = double.Parse(textBoxCost.Text);
					acc.PRODUCT_CD = BankView.db.PRODUCT.Where(x => x.NAME == comboBoxClientOptions.SelectedItem.ToString()).Select(y => y.PRODUCT_CD).FirstOrDefault();
					acc.CUST_ID = cust_id;
					acc.OPEN_EMP_ID = BankView.db.EMPLOYEE.Where(x => x.FIRST_NAME + " " + x.LAST_NAME == comboBoxEmployee.SelectedItem.ToString()).Select(y => y.EMP_ID).FirstOrDefault();
					acc.STATUS = "ACTIVE";
					BankView.db.ACCOUNT.InsertOnSubmit(acc);
					BankView.db.SubmitChanges();
					listBoxProductName.ItemsSource = BankView.db.ACCOUNT.Where(x => x.CUST_ID == cust_id);
				}
				else
				{

					acc = listBoxProductName.SelectedItem as ACCOUNT;
					BankView.db.ACCOUNT.DeleteOnSubmit(acc);
					BankView.db.SubmitChanges();
					//listBoxProductName.ItemsSource = BankView.db.ACCOUNT.Where(x => x.CUST_ID == temp.CUST_ID);
				}
			}
		}

		private void ComboBoxBranch_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			bank.Select = comboBoxBranch.SelectedIndex + 1;
			tabControl.IsEnabled = false;
			listBoxUser.IsEnabled = false;
			//listBoxUser.ItemsSource = BankView.db.CUSTOMER.Where(x => x.CUST_TYPE_CD == "I" && x.CITY == comboBoxBranch.SelectedItem.ToString());
			//listBoxUser.ItemsSource = BankView.db.CUSTOMER.Where(x => x.CUST_TYPE_CD == "I");
			comboBoxEmployee.IsEnabled = true;
			comboBoxEmployee.ItemsSource = BankView.db.EMPLOYEE.Where(x => x.ASSIGNED_BRANCH_ID == bank.Select && x.DEPT_ID == 1).Select(y => y.FIRST_NAME + " " + y.LAST_NAME);
			tabControl.IsEnabled = false;
			listBoxUser.IsEnabled = false;
		}
		private void comboBoxEmployee_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			
			TabControl_SelectionChanged(sender, e);
			tabControl.IsEnabled = true;
			listBoxUser.IsEnabled = true;
		}
		private void Button_Click(object sender, RoutedEventArgs e)
		{
			BankView.db.SubmitChanges();
		}
	}
}
