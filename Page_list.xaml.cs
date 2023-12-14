using CommunityToolkit.Maui.Views;
using Firebase.Database;
using Firebase_modelo_singleton.Models;
using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace Firebase_modelo_singleton
{
    public partial class Page_list : ContentPage
    {
        public static string id_nota;
        public static string audio_record, photo_record, descripcion;
        public static DateTime fecha;

        public ObservableCollection<Notas> PeopleList { get; set; }
        
        public Page_list()
        {
            InitializeComponent();

            PeopleList = new ObservableCollection<Notas>();
            LoadData();

            var sortedList = new List<Notas>(PeopleList);
            sortedList=sortedList.OrderByDescending(i => i.fecha).ToList();

            PeopleList=new ObservableCollection<Notas>(sortedList);

            peopleListView.ItemsSource=PeopleList;  
        }

        protected override async void OnAppearing() {
            base.OnAppearing();

            await LoadData();
        }

        public void SetPeopleList(ObservableCollection<Notas> people)
        {
            PeopleList.Clear();
            foreach (var person in people)
            {
                PeopleList.Add(person);
            }
        }

        private async void peopleListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
                return;

            var selectedPerson = (Notas) e.SelectedItem;

            var action = await DisplayActionSheet($"Opciones para {selectedPerson.id_nota}", "Cancelar", null, "Editar", "Eliminar", "Reproducir audio");

            if(selectedPerson!=null) {
                id_nota=selectedPerson.id_nota;
                audio_record=selectedPerson.audio_record;
                photo_record=selectedPerson.photo_record;
                descripcion=selectedPerson.descripcion;
                fecha=selectedPerson.fecha;
            }

            var firebaseInstance = Singleton.Instance;
            switch (action)
            {
                case "Editar":
                    var pageUpdate = new Page_update();
                    pageUpdate.SetPersonaSeleccionada(selectedPerson);
                    await Navigation.PushAsync(pageUpdate);
                    break;
                case "Eliminar":
                    try
                    {

                    Console.WriteLine("error: "+selectedPerson.id_nota);
                    await firebaseInstance.DeleteData(selectedPerson.id_nota.ToString());
                        await LoadData();
                        await DisplayAlert("Éxito", "Persona eliminada correctamente.", "OK");
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert("Error", $"Error al eliminar persona: {ex.Message}", "OK");
                    }
                    break;

                case "Reproducir audio":
                    
                    try {
                        ReproducirAudio();
                    } catch(Exception ex) {
                        await DisplayAlert("Error",$"Error al eliminar persona: {ex.Message}","OK");
                    }
                break;
            }

            peopleListView.SelectedItem = null;
        }

        private void ReproducirAudio() {
            MediaElement mediaElement = new MediaElement {
                Source=audio_record,
                ShouldAutoPlay=true
            };

            container.Add(mediaElement);
        }


        private async Task LoadData()
        {
            try
            {
                var firebaseInstance = Singleton.Instance;

                var personas = await firebaseInstance.ReadData();

                SetPeopleList(new ObservableCollection<Notas>(personas));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al cargar datos: {ex.Message}", "OK");
            }
        }
    }
}
