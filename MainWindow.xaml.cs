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
using System.Collections;

namespace Banking
{
	/// <summary>
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{

		BankView bank;
		IList select_employee;
		IList select_user;
		IQueryable employee_president = BankView.db.EMPLOYEE.Where(x => x.DEPT_ID == 3).Select(y => y.TITLE + "\n" + y.FIRST_NAME + " " + y.LAST_NAME);
		bool reverseButtonChangeData = false;
		public MainWindow()
		{
			InitializeComponent();

			DataContext = bank = new BankView();

            comboBoxBranch.SelectionChanged += ComboBoxBranch_SelectionChanged;
            tabControl.SelectionChanged += TabControl_SelectionChanged;

			comboBoxNameBranch.ItemsSource = bank.Branch;
			comboBoxDepartment.ItemsSource = BankView.db.DEPARTMENT.Select(x => x.NAME);
			comboBoxPosition.ItemsSource = BankView.db.EMPLOYEE.Select(x => x.TITLE).Distinct();


			comboBoxClientOptions.SelectionChanged += (s, e) => { buttonAddProduct.Content = "Оформить"; buttonAddProduct.IsEnabled = true; };

			listBoxProductName.SelectionChanged += (s, e) =>
			{ 
				buttonAddProduct.IsEnabled = true;
				textBoxCost.IsEnabled = true;
			};
			listBoxProductNameVIP.SelectionChanged += (s, e) => { buttonAddProduct.IsEnabled = true; textBoxCost.IsEnabled = true; };


			comboBoxEmployee.SelectionChanged += (s, e) => { tabControl.IsEnabled = true; };

			listBoxUser.SelectionChanged += (s, e) => 
			{ 
				stackPanelBottom.IsEnabled = true;
				comboBoxClientOptions.SelectedIndex = -1;
				buttonAddProduct.Content = "Погасить/Положить";
				select_user = e.AddedItems;
				buttonNewOfficer.Content = "Save changed data";
				buttonNewCustomer.Content = "Save changed data";
				buttonRemoveCustomer.IsEnabled = true;
				
			};

			tabControl.IsEnabled = false;
		}
		private void ReverseButtonChangeData()
        {
			if(reverseButtonChangeData)
            {
				textBoxEmpFirst.IsReadOnly = false;
				textBoxEmpLast.IsReadOnly = false;
				combobBoxDepartment.Visibility = Visibility.Visible;
				combobBoxPosition.Visibility = Visibility.Visible;
				buttonChangeDate.Content = "Save data";
            }
			else
            {
				textBoxEmpFirst.IsReadOnly = true;
				textBoxEmpLast.IsReadOnly = true;
				combobBoxDepartment.Visibility = Visibility.Hidden;
				combobBoxPosition.Visibility = Visibility.Hidden;
				buttonChangeDate.Content = "To change the data";
				buttonChangeDate.IsEnabled = false;
			}
        }
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
			if (e.Source is ComboBox || e.Source is ListBox)
            {
				if (e.Source == combobBoxDepartment || e.Source == combobBoxPosition ||e.Source == listBoxProductName) return;
            }
			if(comboBoxEmployee.SelectedIndex !=-1)
            if (tabControl.SelectedIndex == 0)
            {
				stackPanelBottom.Visibility = Visibility.Hidden;
					foreach(Object item in employee_president)
						if(select_employee.Contains(item))
                        {
							listBoxUser.ItemsSource = null;
							listBoxUser.ItemsSource = BankView.db.EMPLOYEE.Select(x=>x);
							break;
						}
						else
                        {
							listBoxUser.ItemsSource = BankView.db.EMPLOYEE.Where(x => x.ASSIGNED_BRANCH_ID == bank.Select);
							
						}


            }
            else if (tabControl.SelectedIndex == 1)
            {
					//listBoxUser.ItemsSource = BankView.db.CUSTOMER.Where(x => x.CUST_TYPE_CD == "I" && x.CITY == comboBoxBranch.SelectedItem.ToString()); //Клиенты из этого филиала
				buttonRemoveCustomer.IsEnabled = false;
				stackPanelBottom.Visibility = Visibility.Visible;
				buttonNewCustomer.Content = "Register a new client";
				listBoxUser.ItemsSource = BankView.db.CUSTOMER.Where(x => x.CUST_TYPE_CD == "I");
					comboBoxClientOptions.SelectedIndex = -1;
					comboBoxClientOptions.ItemsSource = BankView.db.PRODUCT.Select(x => x.NAME).Where(y => y.Contains("business") != true);
					buttonAddProduct.IsEnabled = true;
				}
            else if (tabControl.SelectedIndex == 2)//Вип
            {
					//listBoxUser.ItemsSource = BankView.db.CUSTOMER.Where(x => x.CUST_TYPE_CD == "B");
					stackPanelBottom.Visibility = Visibility.Visible;
					comboBoxClientOptions.SelectedIndex = -1;
					listBoxUser.ItemsSource = BankView.db.OFFICER;
                comboBoxClientOptions.ItemsSource = BankView.db.PRODUCT.Select(x => x.NAME);
					buttonAddProduct.IsEnabled = true;
				}
            else if (tabControl.SelectedIndex == 3)
            {
				listBoxUser.ItemsSource = BankView.db.CUSTOMER.Select(x => x.ACCOUNT.Select(y => y.ACC_TRANSACTION.Select(z => z.TELLER_EMP_ID)));
            }
			 //var tt = BankView.db.ACCOUNT.Select(x => x.ACCOUNT_ID);
        }

       

        private void buttonAddProduct_Click(object sender, RoutedEventArgs e)//Добавить кредит - погасить
        {
				int cust_id=-1;
			CUSTOMER cust = null;
			OFFICER offi = null;

			if (textBoxCost.Text.All(x => !char.IsDigit(x)) && comboBoxClientOptions.SelectedIndex!=-1 && comboBoxEmployee.SelectedIndex != -1)
			{ 
				MessageBox.Show("Сумма введена не верно!"); return;
			}

			if (listBoxUser.SelectedItem is CUSTOMER)
			{
				cust = (CUSTOMER)listBoxUser.SelectedItem;

				cust_id = cust.CUST_ID;
			}
			else if (listBoxUser.SelectedItem is OFFICER)
			{
				offi = (OFFICER)listBoxUser.SelectedItem;

				cust_id =(int)BankView.db.OFFICER.Where(x => x.OFFICER_ID == offi.OFFICER_ID).Select(y => y.CUST_ID).FirstOrDefault(); 
			}

			if(cust_id!=-1)
            {
				ACCOUNT acc = new ACCOUNT();
				if (buttonAddProduct.Content.ToString() == "Оформить")
				{
					acc.AVAIL_BALANCE = double.Parse(textBoxCost.Text);
					acc.OPEN_BRANCH_ID = bank.Select;
					acc.OPEN_DATE = DateTime.Now;
					if (comboBoxClientOptions.SelectedItem.ToString() == "Insurance Offerings")
						acc.PENDING_BALANCE = double.Parse(textBoxCost.Text)*5;
					else
						acc.PENDING_BALANCE = double.Parse(textBoxCost.Text);
					acc.PRODUCT_CD = BankView.db.PRODUCT.Where(x => x.NAME == comboBoxClientOptions.SelectedItem.ToString()).Select(y => y.PRODUCT_CD).FirstOrDefault();
					acc.CUST_ID = cust_id;
					acc.OPEN_EMP_ID = BankView.db.EMPLOYEE.Where(x => x.FIRST_NAME + " " + x.LAST_NAME == comboBoxEmployee.SelectedItem.ToString()).Select(y => y.EMP_ID).FirstOrDefault();
					acc.STATUS = "ACTIVE";
					BankView.db.ACCOUNT.InsertOnSubmit(acc);
					BankView.db.SubmitChanges();
					comboBoxClientOptions.SelectedIndex = -1;

					
					
					//listBoxProductName.ItemsSource = BankView.db.ACCOUNT.Where(x => x.CUST_ID == cust_id).Select(y => y.PRODUCT.NAME);
					//listBoxProductName.ItemsSource = BankView.db.ACCOUNT.Where(x => x.CUST_ID == cust_id);
				}
				else if(buttonAddProduct.Content.ToString() == "Погасить/Положить")
				{
					acc = cust == null ? (ACCOUNT)listBoxProductNameVIP.SelectedItem : (ACCOUNT)listBoxProductName.SelectedItem;
					string product_type = BankView.db.PRODUCT.Where(x => x.PRODUCT_CD == acc.PRODUCT_CD).Select(y => y.PRODUCT_TYPE_CD).FirstOrDefault();
					double pending_balance = (double)acc.PENDING_BALANCE;
					double cost = double.Parse(textBoxCost.Text);

					ACC_TRANSACTION transaction = new ACC_TRANSACTION();
					transaction.ACCOUNT_ID = acc.ACCOUNT_ID;
					transaction.EXECUTION_BRANCH_ID = bank.Select;
					transaction.TELLER_EMP_ID = BankView.db.EMPLOYEE.Where(x => x.FIRST_NAME + " " + x.LAST_NAME == comboBoxEmployee.SelectedItem.ToString()).Select(y => y.EMP_ID).FirstOrDefault();
					transaction.AMOUNT = cost;
					transaction.TXN_DATE = DateTime.Now;
					transaction.FUNDS_AVAIL_DATE = DateTime.Now;
					transaction.TXN_TYPE_CD = "CDT";

					if (product_type == "ACCOUNT")//Счета
                    {

						acc.PENDING_BALANCE += cost;
						acc.AVAIL_BALANCE += cost;
						BankView.db.ACC_TRANSACTION.InsertOnSubmit(transaction);
						BankView.db.SubmitChanges();
					}
					else if (product_type == "LOAN")//Кредит-лизинг
                    {
						
						if (pending_balance - cost > 0)
						{
							acc.PENDING_BALANCE -= cost;
							BankView.db.ACC_TRANSACTION.InsertOnSubmit(transaction);
							BankView.db.SubmitChanges();
						}
						else
						{
							acc.CLOSE_DATE = DateTime.Now;
							acc.STATUS = "CLOSE";
							BankView.db.SubmitChanges();
						}

                    }
					
				}
				if (cust == null)
				{
					listBoxProductNameVIP.ItemsSource = null;
					listBoxProductNameVIP.ItemsSource = BankView.db.ACCOUNT.Where(x => x.CUST_ID == cust_id);

				}
				else
				{
					listBoxProductName.ItemsSource = null;
					listBoxProductName.ItemsSource = BankView.db.ACCOUNT.Where(x => x.CUST_ID == cust_id);
				}
			}

			textBoxCost.Text = "";
			textBoxCost.IsEnabled = false;
		}
		
		private void ComboBoxBranch_SelectionChanged(object sender, SelectionChangedEventArgs e)//Выбор банка
		{
			bank.Select = comboBoxBranch.SelectedIndex + 1;
			tabControl.IsEnabled = false;
			listBoxUser.IsEnabled = false;
			buttonChangeDate.Content = "To change the data";
			buttonChangeDate.IsEnabled = false;
			//listBoxUser.ItemsSource = BankView.db.CUSTOMER.Where(x => x.CUST_TYPE_CD == "I" && x.CITY == comboBoxBranch.SelectedItem.ToString());
			//listBoxUser.ItemsSource = BankView.db.CUSTOMER.Where(x => x.CUST_TYPE_CD == "I");
			comboBoxEmployee.IsEnabled = true;

			//comboBoxEmployee.ItemsSource = BankView.db.EMPLOYEE.Where(x => x.ASSIGNED_BRANCH_ID == bank.Select && x.DEPT_ID == 1).Select(y => y.FIRST_NAME + " " + y.LAST_NAME);
			IQueryable employee_teller = BankView.db.EMPLOYEE.Where(x => x.ASSIGNED_BRANCH_ID == bank.Select && x.DEPT_ID == 1).Select(y => y.FIRST_NAME + " " + y.LAST_NAME);
			
			
			ComboBoxItem separator = new ComboBoxItem();
			separator.BorderBrush = Brushes.Black;
			separator.BorderThickness = new Thickness(0, 0, 0, 1);

			comboBoxEmployee.Items.Clear();
			foreach (Object item in employee_teller)
				comboBoxEmployee.Items.Add(item);

			

			if(bank.Select == 1)
            {
				comboBoxEmployee.Items.Add(separator);
				foreach (Object item in employee_president)
				{
					comboBoxEmployee.Items.Add(item);
				}
			}
			

			tabControl.IsEnabled = false;
			listBoxUser.IsEnabled = false;
		}
		private void comboBoxEmployee_SelectionChanged(object sender, SelectionChangedEventArgs e)//Выбор сотрудника
		{

			select_employee = e.AddedItems;

			foreach (Object item in employee_president)//Если не президент добавить-удалить нельзя
				if (select_employee.Contains(item))
				{ 
					stackPanelnewEmployee.IsEnabled = true;
					buttonChangeDate.IsEnabled = true;
					reverseButtonChangeData = true;
					break;
				}
                else 
				{
					reverseButtonChangeData = false;
					ReverseButtonChangeData();
					stackPanelnewEmployee.IsEnabled = false;
				}
			TabControl_SelectionChanged(sender, e);
			tabControl.IsEnabled = true;
			listBoxUser.IsEnabled = true;
		}
		private void Button_Click(object sender, RoutedEventArgs e)
		{
			BankView.db.SubmitChanges();
		}

        private void saveEmployee_Click(object sender, RoutedEventArgs e)//Новый сотрудник
        {
			if (textBoxFirstName.Text != "" && textBoxLastName.Text != "" && comboBoxNameBranch.SelectedIndex != -1 && comboBoxDepartment.SelectedIndex != -1 && comboBoxPosition.SelectedIndex != -1)
			{
				EMPLOYEE employee = new EMPLOYEE();
				employee.FIRST_NAME = textBoxFirstName.Text;
				employee.LAST_NAME = textBoxLastName.Text;
				employee.START_DATE = DateTime.Now;
				employee.TITLE = comboBoxPosition.Text;
				employee.ASSIGNED_BRANCH_ID = BankView.db.BRANCH.Where(x => x.CITY == comboBoxNameBranch.Text).Select(y => y.BRANCH_ID).FirstOrDefault();
				employee.DEPT_ID = BankView.db.DEPARTMENT.Where(x => x.NAME == comboBoxDepartment.Text).Select(y => y.DEPT_ID).FirstOrDefault();
				BankView.db.EMPLOYEE.InsertOnSubmit(employee);
				BankView.db.SubmitChanges();
				listBoxUser.ItemsSource = null;
				listBoxUser.ItemsSource = BankView.db.EMPLOYEE;
				textBoxFirstName.Clear();
				textBoxLastName.Clear();
				comboBoxPosition.SelectedIndex = -1;
				comboBoxDepartment.SelectedIndex = -1;
				comboBoxNameBranch.SelectedIndex = -1;
			}
			else MessageBox.Show("Data entered incorrectly");
        }

        private void removeEmployee_Click(object sender, RoutedEventArgs e)//Удалить сотрудника
        {

            if (listBoxUser.SelectedIndex!=-1)
            {
				EMPLOYEE item = (EMPLOYEE)listBoxUser.SelectedItem;
				if (!comboBoxEmployee.SelectedItem.ToString().Contains(item.FIRST_NAME + " " + item.LAST_NAME))
				{
					
					BankView.db.EMPLOYEE.DeleteOnSubmit(item);
					BankView.db.SubmitChanges();
					listBoxUser.ItemsSource = null;
					listBoxUser.ItemsSource = BankView.db.EMPLOYEE.Select(x => x);
				}
			}

        }

        private void buttonChangeDate_Click(object sender, RoutedEventArgs e)//Изменить данные сотрудника
        {
			if(buttonChangeDate.Content.ToString() == "To change the data")
            {
				ReverseButtonChangeData();
				combobBoxDepartment.ItemsSource = BankView.db.DEPARTMENT.Select(x => x.NAME);
				combobBoxPosition.ItemsSource = BankView.db.EMPLOYEE.Select(x => x.TITLE).Distinct();

            }
			else if(buttonChangeDate.Content.ToString() == "Save data")
            {
				EMPLOYEE item = (EMPLOYEE)listBoxUser.SelectedItem;
				EMPLOYEE emp = BankView.db.EMPLOYEE.Where(x => x.EMP_ID == item.EMP_ID).FirstOrDefault();
					if (combobBoxDepartment.SelectedIndex != -1)
						emp.DEPT_ID = BankView.db.DEPARTMENT.Where(x => x.NAME == combobBoxDepartment.Text.ToString()).Select(y => y.DEPT_ID).FirstOrDefault();
					if (combobBoxPosition.SelectedIndex != -1)
						emp.TITLE = combobBoxPosition.Text;
				BankView.db.SubmitChanges();

            }
        }

        private void buttonRegisterNewCustomer_Click(object sender, RoutedEventArgs e)//Новый клиент //Изменить данные клиента
        {
			
			if(buttonNewCustomer.Content.ToString() == "Register a new client")
            {
				CUSTOMER cust = new CUSTOMER();
				cust.ADDRESS = textBoxAddressClient.Text;
				cust.CITY = textBoxCityClient.Text;
				cust.FED_ID = textBoxFedClient.Text;
				cust.POSTAL_CODE = textBoxPostalCodeClient.Text;
				cust.STATE = BankView.db.BRANCH.Where(x => x.BRANCH_ID == bank.Select).Select(y => y.STATE).FirstOrDefault();
				cust.CUST_TYPE_CD = "I";
				BankView.db.CUSTOMER.InsertOnSubmit(cust);
				BankView.db.SubmitChanges();
				INDIVIDUAL indiv = new INDIVIDUAL();
				indiv.FIRST_NAME = textBoxFirstClient.Text;
				indiv.LAST_NAME = textBoxLastClient.Text;
				indiv.CUST_ID = cust.CUST_ID;
				BankView.db.INDIVIDUAL.InsertOnSubmit(indiv);
				BankView.db.SubmitChanges();

				listBoxUser.ItemsSource = null;
				listBoxUser.ItemsSource = BankView.db.CUSTOMER.Where(x => x.CUST_TYPE_CD == "I");
			}
			else if(buttonNewCustomer.Content.ToString() == "Save changed data")
			{
				BankView.db.SubmitChanges();
			}

        }

        private void buttonRemoveCustomer_Click(object sender, RoutedEventArgs e)//Удалить клиента со всеми аккаунтами
        {
			CUSTOMER cust = (CUSTOMER)listBoxUser.SelectedItem;
			IQueryable<ACCOUNT> acc = BankView.db.ACCOUNT.Where(x => x.CUST_ID == cust.CUST_ID);

			foreach (ACCOUNT item in acc)
			{ 
				IQueryable<ACC_TRANSACTION> tran = BankView.db.ACC_TRANSACTION.Where(x => x.ACCOUNT_ID == item.ACCOUNT_ID);
				BankView.db.ACC_TRANSACTION.DeleteAllOnSubmit(tran);
			}
			BankView.db.ACCOUNT.DeleteAllOnSubmit(acc);

			INDIVIDUAL individ = BankView.db.INDIVIDUAL.Where(x => x.CUST_ID == cust.CUST_ID).FirstOrDefault();


			BankView.db.INDIVIDUAL.DeleteOnSubmit(individ);
			BankView.db.CUSTOMER.DeleteOnSubmit(cust);

			BankView.db.SubmitChanges();

			listBoxUser.ItemsSource = null;
			listBoxUser.ItemsSource = BankView.db.CUSTOMER.Where(x => x.CUST_TYPE_CD == "I");

		}



        private void busttonNewOfficer_Click(object sender, RoutedEventArgs e)
        {
			
			if (buttonNewOfficer.Content.ToString() == "Register new VIPClient")
			{
				CUSTOMER cust = new CUSTOMER();
				cust.ADDRESS = textBoxAddressClient.Text;
				cust.CITY = textBoxCityClient.Text;
				cust.FED_ID = textBoxFedClient.Text;
				cust.POSTAL_CODE = textBoxPostalCodeClient.Text;
				cust.STATE = BankView.db.BRANCH.Where(x => x.BRANCH_ID == bank.Select).Select(y => y.STATE).FirstOrDefault();
				cust.CUST_TYPE_CD = "I";
				BankView.db.CUSTOMER.InsertOnSubmit(cust);
				BankView.db.SubmitChanges();
				OFFICER offi = new OFFICER();
				offi.FIRST_NAME = textBoxFirstClient.Text;
				offi.LAST_NAME = textBoxLastClient.Text;
				offi.CUST_ID = cust.CUST_ID;
				BankView.db.OFFICER.InsertOnSubmit(offi);
				BankView.db.SubmitChanges();

				listBoxUser.ItemsSource = null;
				listBoxUser.ItemsSource = BankView.db.CUSTOMER.Where(x => x.CUST_TYPE_CD == "B");
			}
			else if (buttonNewOfficer.Content.ToString() == "Save changed data")
			{
				BankView.db.SubmitChanges();
			}
		}
	}
}
