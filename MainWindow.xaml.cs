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


			comboBoxClientOptions.SelectionChanged += (s, e) => { buttonAddProduct.Content = "Добавить"; buttonAddProduct.IsEnabled = true; };

			listBoxProductName.SelectionChanged += (s, e) => { buttonAddProduct.IsEnabled = true; };
			comboBoxEmployee.SelectionChanged += (s, e) => { tabControl.IsEnabled = true; };

			listBoxUser.SelectionChanged += (s, e) => 
			{ 
				stackPanelBottom.IsEnabled = true;
				comboBoxClientOptions.SelectedIndex = -1;
				buttonAddProduct.Content = "Онулировать";
				select_user = e.AddedItems;
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
			if (e.Source is ComboBox)
            {
				if (e.Source == combobBoxDepartment || e.Source == combobBoxPosition) return;
            }
			if(comboBoxEmployee.SelectedIndex !=-1)
            if (tabControl.SelectedIndex == 0)
            {
				stackPanelBottom.IsEnabled = false;
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
			separator.Name = "separator";

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
		private void comboBoxEmployee_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

        private void saveEmployee_Click(object sender, RoutedEventArgs e)
        {
			if(textBoxFirstName.Text != "" && textBoxLastName.Text != "" && comboBoxNameBranch.SelectedIndex != -1 && comboBoxDepartment.SelectedIndex != -1 && comboBoxPosition.SelectedIndex != -1)
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
        }

        private void removeEmployee_Click(object sender, RoutedEventArgs e)
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

        private void buttonChangeDate_Click(object sender, RoutedEventArgs e)
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
					emp.FIRST_NAME = textBoxEmpFirst.Text;
					emp.LAST_NAME = textBoxEmpLast.Text;
                
				BankView.db.SubmitChanges();

            }
        }
    }
}
