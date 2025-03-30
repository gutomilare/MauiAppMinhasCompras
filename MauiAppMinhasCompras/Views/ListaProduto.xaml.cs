using MauiAppMinhasCompras.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MauiAppMinhasCompras.Views;

public partial class ListaProduto : ContentPage
{
	ObservableCollection<Produto> lista = new ObservableCollection<Produto>();

	public ListaProduto()
	{
		InitializeComponent();

		lst_produtos.ItemsSource = lista;
	}

    protected async override void OnAppearing()
    {
		try
		{
			lista.Clear();

			List<Produto> tmp = await App.Db.GetAll();

			tmp.ForEach(i => lista.Add(i));
		}
		catch (Exception ex)
		{
			await DisplayAlert("Ops", ex.Message, "OK");
		}
    }

    private void ToolbarItem_Clicked(object sender, EventArgs e)
    {
		try
		{
			Navigation.PushAsync(new Views.NovoProduto());
		} 
		catch (Exception ex)
		{
			DisplayAlert("Ops", ex.Message, "OK");
		}
    }

    private async void txt_search_TextChanged(object sender, TextChangedEventArgs e)
    {
		try
		{
			string q = e.NewTextValue;

			lst_produtos.IsRefreshing = true;

			lista.Clear();

			List<Produto> tmp = await App.Db.Search(q);

			tmp.ForEach(i => lista.Add(i));
		}
		catch (Exception ex)
		{
			await DisplayAlert("Ops", ex.Message, "OK");
		}
		finally
		{
			lst_produtos.IsRefreshing = false;
		}
    }

    private void ToolbarItem_Clicked_1(object sender, EventArgs e)
    {
		try
		{
            double soma = lista.Sum(i => i.Total);

            string msg = $"O total é {soma:C}";

            DisplayAlert("Total dos Produtos", msg, "OK");
        }
		catch (Exception ex) 
		{
			DisplayAlert("Ops", ex.Message, "OK");
		}
    }

    private async void MenuItem_Clicked(object sender, EventArgs e)
    {
		try
		{
			MenuItem selecionado = sender as MenuItem;

			Produto p = selecionado.BindingContext as Produto;

			bool confirm = await DisplayAlert("Tem certeza?", $"Remover {p.Descricao}?", "Sim", "Não");

			if (confirm)
			{
				await App.Db.Delete(p.Id);
				lista.Remove(p);
			}
		}
		catch (Exception ex) 
		{
			await DisplayAlert("Ops", ex.Message, "OK");
		}
    }

    private void lst_produtos_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
		try
		{
			Produto p = e.SelectedItem as Produto;

			Navigation.PushAsync(new Views.EditarProduto
			{
				BindingContext = p,
			});
		}
		catch (Exception ex)
		{
			DisplayAlert("Ops", ex.Message, "OK");
		}
    }

    private async void lst_produtos_Refreshing(object sender, EventArgs e)
    {
        try
        {
            lista.Clear();

            List<Produto> tmp = await App.Db.GetAll();

            tmp.ForEach(i => lista.Add(i));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ops", ex.Message, "OK");
        }
		finally
		{
			lst_produtos.IsRefreshing = false;
		}
    }

    private async void ToolbarItem_Clicked_2(object sender, EventArgs e)
    {
		try
		{
			var produtos = await App.Db.GetAll();

			var categorias = produtos
				.Where(p => !string.IsNullOrEmpty(p.Categoria))
				.Select(p => p.Categoria)
				.Distinct()
				.OrderBy(c => c)
				.ToList();

			categorias.Insert(0, "Todas");

            string opcaoSelecionada = await DisplayActionSheet("Filtrar por Categoria", "Cancelar", null, categorias.ToArray());

            if (opcaoSelecionada == "Cancelar" || string.IsNullOrWhiteSpace(opcaoSelecionada))
                return;

            if (opcaoSelecionada == "Todas")
            {
                lst_produtos.ItemsSource = produtos;
            }
            else
            {
                lst_produtos.ItemsSource = produtos.Where(p => p.Categoria == opcaoSelecionada).ToList();
            }
        }
		catch (Exception ex)
		{
            await DisplayAlert("Ops", ex.Message, "OK");
        }
    }

    private async void ToolbarItem_Clicked_3(object sender, EventArgs e)
    {
		try
		{
			var produtos = await App.Db.GetAll();

			var relatorio = produtos
				.Where(p => !string.IsNullOrEmpty(p.Categoria))
				.GroupBy(p => p.Categoria)
				.Select(g => new
				{
					Categoria = g.Key,
					Total = g.Sum(p => p.Total)
				}).ToList();

			if (relatorio.Count == 0)
			{
				await DisplayAlert("Relatório", "Nenhum dado para exibir.", "Ok");
				return;
			}

			string mensagem = string.Join("\n", relatorio.Select(r => $"{r.Categoria}: R$ {r.Total:F2}"));

			await DisplayAlert("Gastos por Categoria", mensagem, "Ok");

		}
		catch (Exception ex)
		{
            await DisplayAlert("Ops", ex.Message, "OK");
        }
    }
}