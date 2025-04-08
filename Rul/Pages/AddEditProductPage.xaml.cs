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
using Microsoft.Win32;
using Rul.Entities;

namespace Rul.Pages
{
    /// <summary>
    /// Логика взаимодействия для AddEditProductPage.xaml
    /// </summary>
    public partial class AddEditProductPage : Page
    {
        Product product = new Product();

        public AddEditProductPage(Product currentProduct)
        {
            InitializeComponent();

            if (currentProduct != null) 
            {
                product = currentProduct;

                btnDeleteProduct.Visibility = Visibility.Visible;
                txtArticle.IsEnabled = false;
            }
            DataContext = product;
            cmbCategory.ItemsSource = CategoryList;
        }

        public string[] CategoryList =
        {
            "Аксессуары",
            "Автозапчасти",
            "Автосервис",
            "Съемники подшипников",
            "Ручные инструменты",
            "Зарядные устройства"
        };

        private void btnEnterImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog getImageDialog = new OpenFileDialog();

            getImageDialog.Filter = "Файды изображений: (*.png, *.jpg, *.jpeg)| *.png; *.jpg; *.jpeg";
            getImageDialog.InitialDirectory = "D:\\programmesmodules\\Rul\\Rul\\Resources";
            if (getImageDialog.ShowDialog() == true)
            {
                product.ProductImage = getImageDialog.SafeFileName;
                img.Source = new BitmapImage(new Uri(getImageDialog.FileName));
            }
        }

        private void btnDeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show($"Вы действительно хотите удалить {product.ProductName}?", "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    TradeEntities.GetContext().Product.Remove(product);
                    TradeEntities.GetContext().SaveChanges();
                    MessageBox.Show("Запись удалена!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    NavigationService.GoBack();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString(), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnSaveProduct_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            TradeEntities db = new TradeEntities();

            if (product.ProductCost < 0)
                errors.AppendLine("Стоимость не может быть отрицательной!");
            if (product.MinCount < 0)
                errors.AppendLine("Минимальное количество не может быть отрицательным!");
            if (product.ProductDiscountAmount > product.MaxDiscountAmount)
                errors.AppendLine("Действующая скидка на товар не может быть больше максимальной скидки!");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }

            if (!db.Product.Any(x => x.ProductArticleNumber == txtArticle.Text))
            {
                try
                {
                    db.Product.Add(product);
                    db.SaveChanges();
                    MessageBox.Show("Информация сохранена!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    NavigationService.GoBack();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString(), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                try
                {
                    var product = db.Product.FirstOrDefault(x => x.ProductArticleNumber == txtArticle.Text);
                    product.ProductManufacturer = txtManufacturer.Text;
                    product.ProductCategory = cmbCategory.Text;
                    product.ProductQuantityInStock = Convert.ToInt32(txtCountInStock.Text);
                    product.Unit = txtUnit.Text;
                    product.ProductDiscountAmount = Convert.ToByte(txtDiscount.Text);
                    product.Supplier = txtSupplier.Text;
                    product.MaxDiscountAmount = Convert.ToByte(txtMaxDiscount.Text);
                    db.SaveChanges();
                    MessageBox.Show("Информация сохранена!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString(), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
