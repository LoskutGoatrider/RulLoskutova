using System;
using System.Collections.Generic;
using System.Linq;
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
using Rul.Entities;
using Rul.Windows;

namespace Rul.Pages
{
    /// <summary>
    /// Логика взаимодействия для Admin.xaml
    /// </summary>
    public partial class Admin : Page
    {
        User user = new User();

        public Admin(User currentUser)
        {
            InitializeComponent();

            var product = TradeEntities.GetContext().Product.ToList();
            LViewProduct.ItemsSource = product;
            DataContext = this;

            txtAllAmount.Text = product.Count.ToString();

            user = currentUser;

            UpdateData();
            User();
        }

        private void User()
        {
            if (user != null)
            {
                txtFullName.Text = $"{user.UserSurname.ToString()} {user.UserName.ToString()} {user.UserPatronymic.ToString()}";
            }
            else
            {
                txtFullName.Text = "Гость";
            }
        }

        public string[] SortingList { get; set; } =
        {
            "Без сортировки",
            "Стоимость по возрастанию",
            "Стоимость по убыванию"
        };

        public string[] FilterList { get; set; } =
        {
            "Все диапазоны",
            "0% - 9,99%",
            "10% - 14,99%",
            "15% и более"
        };

        private void UpdateData()
        {
            var result = TradeEntities.GetContext().Product.ToList();

            if (cmbSorting.SelectedIndex == 1)
            {
                result = result.OrderBy(p => p.ProductCost).ToList();
            }
            if (cmbSorting.SelectedIndex == 2)
            {
                result = result.OrderByDescending(p => p.ProductCost).ToList();
            }

            if (cmbFilter.SelectedIndex == 1)
            {
                result = result.Where(p => p.ProductDiscountAmount >= 0 && p.ProductDiscountAmount < 10).ToList();
            }
            if (cmbFilter.SelectedIndex == 2)
            {
                result = result.Where(p => p.ProductDiscountAmount >= 10 && p.ProductDiscountAmount < 15).ToList();
            }
            if (cmbFilter.SelectedIndex == 3)
            {
                result = result.Where(p => p.ProductDiscountAmount >= 15).ToList();
            }

            result = result.Where(p => p.ProductName.ToLower().Contains(txtSearch.Text.ToLower())).ToList();

            txtResultAmount.Text = result.Count.ToString();

            LViewProduct.ItemsSource = result;
        }

        private void txtSearch_SelectionChanged(object sender, RoutedEventArgs e)
        {
            UpdateData();
        }

        private void cmbSorting_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateData();
        }

        private void cmbFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateData();
        }

        List<Product> orderProducts = new List<Product>();
        private void btnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            orderProducts.Add(LViewProduct.SelectedItem as Product);

            if (orderProducts.Count > 0)
            {
                btnOrder.Visibility = Visibility.Visible;
            }
        }

        private void btnOrder_Click(object sender, RoutedEventArgs e)
        {
            OrderWindow orderWindow = new OrderWindow(orderProducts, user);
            orderWindow.Show();
        }

        private void addNewProduct_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddEditProductPage(null));
        }

        private void LViewProduct_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            NavigationService.Navigate(new AddEditProductPage(LViewProduct.SelectedItem as Product));
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility == Visibility.Visible) 
            {
                TradeEntities.GetContext().ChangeTracker.Entries().ToList().ForEach(p => p.Reload());
                LViewProduct.ItemsSource =TradeEntities.GetContext().Product.ToList();
            }
        }
    }
}
