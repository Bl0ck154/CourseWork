﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;

namespace MedicalApp
{
    /// <summary>
    /// Interaction logic for PatientCard.xaml
    /// </summary>
    public partial class PatientCardWindow : Window
    {
        private List<MedicalDoc> documentsOfTheCurrentPatient = null;   // TODO HACK ???

        private int idPatient;

        public PatientCardWindow()
        {
            InitializeComponent();
        }

        public PatientCardWindow(int idPatient)
        {
            InitializeComponent();

            this.idPatient = idPatient;

            this.btnDocEdit.IsEnabled = false;

            this.FillTheCardWithPatientData();

            this.dataGridDocumentList.SelectionChanged += DataGridDocumentList_SelectionChanged;
            //this.dataGridDocumentList.AutoGeneratingColumn += DataGridDocumentList_AutoGeneratingColumn;

            // Button
            this.btnDocAdd.Click += BtnDocAdd_Click;
            this.btnDocEdit.Click += BtnDocEdit_Click;


            // Temp method - do not delete.
            TempMethod();
        }

        private void TempMethod()
        {
            this.dataGridDocumentList.ToolTip = "// TODO = Вместо цифр будут выводится названия типов доков, и в 1ю колонку";
        }

        private void BtnDocEdit_Click(object sender, RoutedEventArgs e)
        {
            int currentDocId
                = (this.dataGridDocumentList.SelectedItem as MedicalDoc)
                .Id;

            AddEditDocument addDocument 
                = new AddEditDocument(this.idPatient, currentDocId);

            addDocument.ShowDialog();

            this.ShowPatientDocsToADatagrid();
        }

        private void BtnDocAdd_Click(object sender, RoutedEventArgs e)
        {
            AddEditDocument addDocument = new AddEditDocument(this.idPatient);

            addDocument.ShowDialog();

            this.ShowPatientDocsToADatagrid();
        }

        

        //private void DataGridDocumentList_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        //{
        //    PropertyDescriptor propertyDescriptor = (PropertyDescriptor)e.PropertyDescriptor;
        //    e.Column.Header = propertyDescriptor.DisplayName;
        //    if (IsColumnNotDisplayed(propertyDescriptor))
        //    {
        //        e.Cancel = true;
        //    }
        //}

        //private static bool IsColumnNotDisplayed(PropertyDescriptor propertyDescriptor)
        //{
        //    if (propertyDescriptor.DisplayName == "Id"
        //        || propertyDescriptor.DisplayName == "idPacient"
        //        || propertyDescriptor.DisplayName == "BeginTime"
        //        || propertyDescriptor.DisplayName == "EndTime"
        //        || propertyDescriptor.DisplayName == "Info"
        //        || propertyDescriptor.DisplayName == "Pacient_Id")
        //    {
        //        return true;
        //    }

        //    return false;
        //}

        private void DataGridDocumentList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.IsARowOfTheDocumentInTheTableIsHighlighted())
            {
                this.ActivationOfTheDocumentEditingButton();

                //this.txbInfo.Text
                    //= ((sender as DataGrid).SelectedItem as MedicalDoc).Info;
            }
            else
            {
                this.DeactivationOfTheDocumentEditingButton();
            }
        }

        /// <summary>
        /// A row of the document in the table is highlighted.
        /// </summary>
        /// <returns>true, if selected.</returns>
        private bool IsARowOfTheDocumentInTheTableIsHighlighted()
        {
            if (this.dataGridDocumentList.SelectedIndex != -1)
            {
                return true;
            }

            return false;
        }

        private void DeactivationOfTheDocumentEditingButton()
        {
            this.btnDocEdit.IsEnabled = false;
        }

        private void ActivationOfTheDocumentEditingButton()
        {
            this.btnDocEdit.IsEnabled = true;
        }

        /// <summary>
        /// Fill the card with patient data.
        /// </summary>
        private void FillTheCardWithPatientData()
        {
            // TODO #1

            using (DataModel db = new DataModel())
            {
                var currentPatient 
                    = (
                    from patient in db.Pacients
                    where patient.Id == this.idPatient
                    select patient
                    )
                    .FirstOrDefault();

                if (currentPatient != null)
                {
                    this.labelFullName.Content = currentPatient.FirstName
                        + " "
                        + currentPatient.LastName
                        // TODO add MiddleName in DB
                        + " "
                        + currentPatient.MiddleName
                        ;

                    this.DateOfBirthValue.Content = currentPatient.BirthDay.ToShortDateString();

                    this.txbAdress.Text = currentPatient.Addres;


                    this.ShowPatientDocsToADatagrid(db);
                }
            }
        }

        private void ShowPatientDocsToADatagrid()
        {
            using (DataModel db = new DataModel())
            {
                this.LoadingFromDatabaseDocsThePatientInDatagrid(db);
            }
        }

        private void ShowPatientDocsToADatagrid(DataModel db)
        {
            this.LoadingFromDatabaseDocsThePatientInDatagrid(db);
        }

        private void LoadingFromDatabaseDocsThePatientInDatagrid(DataModel db)
        {
            // db search doc type
            var documentsOfTheCurrentPatient
            //this.documentsOfTheCurrentPatient
                = (
                from doc in db.MedicalDocs.Include("MedicalDocType")
                //join docType in db.MedicalDocTypes
                //on doc.idMedicalDocType equals docType.Id
                where doc.PatientId == this.idPatient
                //select doc
                select new { doc.Id, DocumentType = doc.MedicalDocType.Name, doc.Name, doc.Info }
                )
                .ToList();

            // show docs patient
            this.dataGridDocumentList.ItemsSource
                = documentsOfTheCurrentPatient
                .Select(x => x)
                //.Select(x => new { x.Id, Type = x.MedicalDocType.Name })
                .ToList();
        }

		private void btnDocSearch_Click(object sender, RoutedEventArgs e)
		{
            // TODO реализовать поиск документов.
            MessageBox.Show("Пока еще не реализовано");
		}
	}
}
