﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        //private List<RedefinedMedicalDoc> documentsOfTheCurrentPatient = null;   // TODO HACK ???
        List<RedefinedMedicalDoc> documentsAfterSearch = null;

        private int idPatient;

        private int selectedIndexDocument;

        //private Type type = null;
        private class RedefinedMedicalDoc /*: MedicalDoc*/
        {
            public int Id { get; set; }
            public string DocumentType { get; set; }
            public string Name { get; set; }
            public string Info { get; set; }
            public DateTime BeginTime { get; set; }
            public DateTime? EndTime { get; set; }
        }

        public PatientCardWindow()
        {
            InitializeComponent();

            this.Loaded += PatientCardWindow_Loaded;
        }

        private void PatientCardWindow_Loaded(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("An error occurred while getting the patient's Id.\n"
                + "The window will be closed.",
                "Error while retrieving data",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            this.Close();
        }

        public PatientCardWindow(int idPatient)
        {
            InitializeComponent();

            this.idPatient = idPatient;

            this.btnDocEdit.IsEnabled = false;

            this.FillTheCardWithPatientData();

            this.datePicStartData.ToolTip = "Enter the date in the format 00.00.0000";
            this.datePicFinalData.ToolTip = "Enter the date in the format 00.00.0000";

            this.documentsAfterSearch = new List<RedefinedMedicalDoc>();




            this.dataGridDocumentList.SelectionChanged += DataGridDocumentList_SelectionChanged;

            // Button
            this.btnDocAdd.Click += BtnDocAdd_Click;
            this.btnDocEdit.Click += BtnDocEdit_Click;
            this.buttonEraser.Click += ButtonEraser_Click;

            // DatePicker
            //this.datePicStartData.PreviewTextInput += DatePicStartData_PreviewTextInput;
            //this.datePicStartData.KeyDown += DatePicStartData_KeyDown;
            this.datePicStartData.PreviewKeyDown += DatePic_PreviewKeyDown;
            this.datePicFinalData.PreviewKeyDown += DatePic_PreviewKeyDown;


            // Temp method - do not delete.
            TempMethod();
        }

        private void ButtonEraser_Click(object sender, RoutedEventArgs e)
        {
            this.txbName.Text = "";
            this.datePicStartData.Text = "";
            this.datePicFinalData.Text = "";
        }

        private void DatePic_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key >= Key.D0 && e.Key <= Key.D9
                || e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9
                || e.Key == Key.OemPeriod
                || e.Key == Key.Decimal
                || e.Key == Key.Back
                || e.Key == Key.Tab)
            {
                e.Handled = false;
            }
            else if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
            else
            {
                e.Handled = true;
            }


        }

        private void TempMethod()
        {
            
        }

        private void BtnDocEdit_Click(object sender, RoutedEventArgs e)
        {
            int currentDocId
                = (this.dataGridDocumentList.SelectedItem as RedefinedMedicalDoc)
                .Id;

            AddEditDocument addDocument
                = new AddEditDocument(this.idPatient, currentDocId);

            bool? result = addDocument.ShowDialog();

            if (result == true)
            {
                this.ShowNotification("Document edited");
            }

            this.ReturningLatestDataInTable();

            // TODO выбрать (выделить) редактируемого пациента
            this.dataGridDocumentList.SelectedIndex = this.selectedIndexDocument;
        }

        /// <summary>
        /// Returning the latest data in the table.
        /// </summary>
        private void ReturningLatestDataInTable()
        {
            if (this.documentsAfterSearch.Count > 0)
            {
                this.dataGridDocumentList.ItemsSource = this.documentsAfterSearch;
            }
            else
            {
                this.ShowPatientDocsToADatagrid();
            }
        }

        private void BtnDocAdd_Click(object sender, RoutedEventArgs e)
        {
            AddEditDocument addDocument = new AddEditDocument(this.idPatient);

            bool? result = addDocument.ShowDialog();

            

            // TODO выбрать (выделить) добавленного пациента
            if (result == true)
            {
                this.documentsAfterSearch.Clear();

                this.ShowNotification("Document added");

                this.ShowPatientDocsToADatagrid();

                this.dataGridDocumentList.SelectedIndex = this.dataGridDocumentList.Items.Count - 1;

                this.dataGridDocumentList.ScrollIntoView(this.dataGridDocumentList.SelectedItem);
            }
            else
            {
                this.ReturningLatestDataInTable();

                this.dataGridDocumentList.SelectedIndex = this.selectedIndexDocument;
            }
        }

        private void DataGridDocumentList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.IsARowOfTheDocumentInTheTableIsHighlighted())
            {
                this.ActivationOfTheDocumentEditingButton();

                this.txbInfo.Text
                    = ((sender as DataGrid).SelectedItem as RedefinedMedicalDoc).Info;

                this.selectedIndexDocument = this.dataGridDocumentList.SelectedIndex;
            }
            else
            {
                this.DeactivationOfTheDocumentEditingButton();

                this.txbInfo.Text = "";
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
                    this.FillTheCardWithData(currentPatient);

                    this.ShowPatientDocsToADatagrid(db);
                }
            }
        }

        /// <summary>
        /// Fill the card with data.
        /// </summary>
        /// <param name="currentPatient"></param>
        private void FillTheCardWithData(Patient currentPatient)
        {
            this.labelFullName.Content = currentPatient.FirstName
                                    + " "
                                    + currentPatient.LastName
                                    + " "
                                    + currentPatient.MiddleName
                                    ;

            this.DateOfBirthValue.Content = currentPatient.BirthDay.ToShortDateString();

            this.txbAdress.Text = currentPatient.Addres;
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
                = (
                from doc in db.MedicalDocs.Include("MedicalDocType")
                where doc.PatientId == this.idPatient
                select new RedefinedMedicalDoc{
                    Id =  doc.Id,
                    DocumentType = doc.MedicalDocType.Name,
                    Name = doc.Name,
                    Info = doc.Info,
                    BeginTime = doc.BeginTime,
                    EndTime = doc.EndTime
                }
                )
                .ToList();


            this.dataGridDocumentList.ItemsSource
                = documentsOfTheCurrentPatient;
        }

		private void btnDocSearch_Click(object sender, RoutedEventArgs e)
		{
            this.selectedIndexDocument = -1;

            if (this.IsSearchFieldsAreEmpty())
            {
                this.ShowPatientDocsToADatagrid();

                this.ForgetTheLastSelectedDocument();
            }
            else
            {
                this.SearchDocumentsBasedOnEnteredData();
            }
        }

        private bool IsCorrectDateRange()
        {
            DateTime dateTimeStartData = this.GetStartingDateOfSearchRange();
            DateTime dateTimeFinalData = this.GetFinalDateOfSearchRange();

            if (dateTimeStartData <= dateTimeFinalData)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Forget the last selected document.
        /// </summary>
        private void ForgetTheLastSelectedDocument()
        {
            this.selectedIndexDocument = -1;
        }

        /// <summary>
        /// Search for patient documents based on entered data.
        /// </summary>
        private void SearchDocumentsBasedOnEnteredData()
        {
            this.documentsAfterSearch.Clear();

            using (DataModel db = new DataModel())
            {
                this.SearchDocumentsByName(db);

                this.SearchDocumentsByDateRange(db);
            }

            this.dataGridDocumentList.ItemsSource
                    = this.documentsAfterSearch;
        }

        /// <summary>
        /// Search documents by date range.
        /// </summary>
        /// <param name="db">Context database.</param>
        private void SearchDocumentsByDateRange(DataModel db)
        {
            if (!String.IsNullOrEmpty(this.datePicStartData.Text)
                                && !String.IsNullOrEmpty(this.datePicFinalData.Text))
            {
                //MessageBox.Show(this.datePicStartData.ToString());

                this.SearchByDateRange(db/*, ref documentsOfTheCurrentPatient*/);
            }
            else if (String.IsNullOrEmpty(this.datePicStartData.Text)
                    && !String.IsNullOrEmpty(this.datePicFinalData.Text)
                    || !String.IsNullOrEmpty(this.datePicStartData.Text)
                    && String.IsNullOrEmpty(this.datePicFinalData.Text))
            {
                MessageBox.Show("To search by range, you need to enter both dates!",
                    "Date field is not filled",
                    MessageBoxButton.OK,
                    MessageBoxImage.Asterisk);
            }
        }

        /// <summary>
        /// Search documents by name.
        /// </summary>
        /// <param name="db">Context database.</param>
        private void SearchDocumentsByName(DataModel db)
        {
            if (!String.IsNullOrEmpty(this.txbName.Text))
            {
                this.documentsAfterSearch
                    = (
                    from doc in db.MedicalDocs.Include("MedicalDocType")
                    where doc.PatientId == this.idPatient
                    where doc.Name.Contains(this.txbName.Text)
                    select new RedefinedMedicalDoc
                    {
                        Id = doc.Id,
                        DocumentType = doc.MedicalDocType.Name,
                        Name = doc.Name,
                        Info = doc.Info,
                        BeginTime = doc.BeginTime,
                        EndTime = doc.EndTime
                    }
                    )
                    .ToList();
            }
        }

        /// <summary>
        /// Search by date range.
        /// </summary>
        /// <param name="db">Context database.</param>
        private void SearchByDateRange(DataModel db)
        {
            if (this.IsCorrectDateRange())
            {
                if (this.documentsAfterSearch.Count > 0)
                {
                    this.documentsAfterSearch
                        = SearchByDateRange(this.documentsAfterSearch);
                }
                else
                {
                    this.documentsAfterSearch
                        = SearchByDateRange(db.MedicalDocs);
                }
            }
            else
            {
                MessageBox.Show("Invalid date range!",
                        "Invalid date range",
                        MessageBoxButton.OK,
                        MessageBoxImage.Asterisk);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="medicalDocs"></param>
        /// <returns></returns>
        private List<RedefinedMedicalDoc> SearchByDateRange(DbSet<MedicalDoc> medicalDocs)
        {
            List<RedefinedMedicalDoc> documents = null;

            DateTime dateTimeStartData = this.GetStartingDateOfSearchRange();
            DateTime dateTimeFinalData = this.GetFinalDateOfSearchRange();

            documents
                = (
                from doc in medicalDocs.Include("MedicalDocType")
                where doc.PatientId == this.idPatient
                where doc.BeginTime >= dateTimeStartData
                where doc.BeginTime <= dateTimeFinalData
                select new RedefinedMedicalDoc
                {
                    Id = doc.Id,
                    DocumentType = doc.MedicalDocType.Name,
                    Name = doc.Name,
                    Info = doc.Info,
                    BeginTime = doc.BeginTime,
                    EndTime = doc.EndTime
                }
                )
                .ToList();

            return documents;
        }

        private List<RedefinedMedicalDoc> SearchByDateRange(List<RedefinedMedicalDoc> documentsOfTheCurrentPatient)
        {
            List<RedefinedMedicalDoc> documents = null;

            DateTime dateTimeStartData = this.GetStartingDateOfSearchRange();
            DateTime dateTimeFinalData = this.GetFinalDateOfSearchRange();

            documents
                = (
                from doc in documentsOfTheCurrentPatient
                where doc.BeginTime >= dateTimeStartData
                where doc.BeginTime <= dateTimeFinalData
                select new RedefinedMedicalDoc
                {
                    Id = doc.Id,
                    DocumentType = doc.DocumentType,
                    Name = doc.Name,
                    Info = doc.Info,
                    BeginTime = doc.BeginTime,
                    EndTime = doc.EndTime
                }
                )
                .ToList();

            return documents;
        }

        /// <summary>
        /// Get the final date of the search range.
        /// </summary>
        /// <returns>Final date of the search range.</returns>
        private DateTime GetFinalDateOfSearchRange()
        {
            return this.datePicFinalData.SelectedDate.Value;
        }

        /// <summary>
        /// Get the starting date of the search range.
        /// </summary>
        /// <returns>Starting date of the search range.</returns>
        private DateTime GetStartingDateOfSearchRange()
        {
            return this.datePicStartData.SelectedDate.Value;
        }



        /// <summary>
        /// Search fields are empty.
        /// </summary>
        /// <returns>true if all search fields are empty.</returns>
        private bool IsSearchFieldsAreEmpty()
        {
            if (String.IsNullOrEmpty(this.txbName.Text)
                && String.IsNullOrEmpty(this.datePicStartData.Text)
                && String.IsNullOrEmpty(this.datePicFinalData.Text))
            {
                return true;
            }

            return false;
        }

        void ShowNotification(string message)
        {
            // SnackbarThree - xaml name of MaterialDesign.Snackbar  
            var messageQueue = SnackbarThree.MessageQueue;

            // The message queue can be called from any thread.
            Task.Run(() => messageQueue.Enqueue(message));
        }
    }
}
