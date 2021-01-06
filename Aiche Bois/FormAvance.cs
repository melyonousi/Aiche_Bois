﻿using System;
using System.Data.OleDb;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace Aiche_Bois
{
    public partial class FormAvance : Form
    {
        /// <summary>
        /// c'est l'access a extereiur Client data base
        /// </summary>
        private readonly OleDbConnection connection = new OleDbConnection();

        /// <summary>
        /// perndre l'id de formulaire presedent;
        /// </summary>
        private long idClient;

        /// <summary>
        /// c'est le variable pour verifier le check box
        /// </summary>
        bool checkAvance;

        /// <summary>
        /// c'est variables pour prendre les valeur des champs
        /// </summary>
        double avance = 0;
        double rest = 0;

        /// <summary>
        /// c'est formulaire de traitment
        /// </summary>
        /// <param name="idClient"></param>
        public FormAvance(String idClient)
        {
            /*
             * get idClient from fomr client
             */

            this.idClient = Convert.ToInt64(idClient);

            /*
             * get database path
             */
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/factures";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            /*
             * set database connection
             */
            connection.ConnectionString = "Provider = Microsoft.ACE.OLEDB.12.0; Data Source = " + path + "/aicheBois.accdb";

            InitializeComponent();
        }

        /// <summary>
        /// charger les donnees a partir de l'id prendre
        /// </summary>
        public void remplirData()
        {
            /**
             * upload database id
             */
            try
            {
                connection.Open();
                OleDbCommand command = new OleDbCommand
                {
                    Connection = connection,
                    CommandText = "SELECT * from client where idClient = " + idClient
                };

                OleDbDataReader readerClient = command.ExecuteReader();
                while (readerClient.Read())
                {
                    lblIDClient.Text = "N" + Convert.ToInt32(readerClient["idClient"]).ToString("D4");
                    lblNomClient.Text = readerClient["nomClient"].ToString();
                    lblDateClient.Text = Convert.ToDateTime(readerClient["dateClient"]).ToString("d");
                    checkAvance = Convert.ToBoolean((readerClient["chAvance"]).ToString());
                    lblAvancePrixClient.Text = Convert.ToDouble(readerClient["prixTotalAvance"]).ToString("F2");
                    lblRestPrixClient.Text = Convert.ToDouble(readerClient["prixTotalRest"]).ToString("F2");
                    lblTotalPrixClient.Text = Convert.ToDouble(readerClient["prixTotalClient"]).ToString("F2");
                }

                connection.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error:: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// l'initialisitaion des champs
        /// </summary>
        private void method_checkavance()
        {
            lblav.Visible = false;
            lblAvancePrixClient.Visible = false;
            lblRestPrixClient.Width = 444;
            txtPrixCLient.Enabled = false;
            txtPrixCLient.Text = "0";
            lblAvancePrixClient.Enabled = false;
            lblAvancePrixClient.Text = "0";
            lblRestPrixClient.Enabled = false;
            lblRestPrixClient.Text = "0";
            txtPrixCLient.Enabled = false;
            txtPrixCLient.Text = "0";
            txtPrixCLient.Visible = false;
            /*btnAnnuler.Visible = false;*/
            /*btnConfirm.Text = "OK";*/
            /*btnConfirm.Width = 444;*/
            btnConfirm.Enabled = false;
            lblPPr.Text = "Le montant a été entièrement payé";
            lblPPr.TextAlign = ContentAlignment.MiddleCenter;
        }

        /// <summary>
        /// on affiche la formulaire
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Avance_Load(object sender, EventArgs e)
        {
            /*
             * upload data from data base
             */
            remplirData();
            btnConfirm.Enabled = false;
            /*
             * check if item checked
             */
            if (!checkAvance)
            {
                method_checkavance();
                return;
            }
        }

        /// <summary>
        /// c'est button modifeir le prix avance est confirmer le triatmenet
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnConfirm_Click(object sender, EventArgs e)
        {
            if (double.Parse(txtPrixCLient.Text) > double.Parse(lblRestPrixClient.Text))
            {
                MessageBox.Show("Le Prix ​​à payer est plus grand a le reste", "Attention", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPrixCLient.Clear();
                txtPrixCLient.Focus();
                return;
            }

            /**
             * updating database id
             */
            try
            {
                connection.Open();

                avance = double.Parse(lblAvancePrixClient.Text);
                rest = double.Parse(lblRestPrixClient.Text);

                avance += double.Parse(txtPrixCLient.Text);
                rest = double.Parse(lblTotalPrixClient.Text) - avance;

                lblAvancePrixClient.Text = avance.ToString("F2");
                lblRestPrixClient.Text = rest.ToString("F2");

                if (double.Parse(lblRestPrixClient.Text) == 0)
                {
                    checkAvance = false;
                    rest = 0;
                    avance = 0;
                    method_checkavance();
                }

                OleDbCommand command = new OleDbCommand
                {
                    Connection = connection,
                    CommandText = "UPDATE client SET prixTotalRest = " + rest + ", prixTotalAvance = " + avance + ", chAvance = " + checkAvance.ToString() + " where idClient = " + idClient
                };

                command.ExecuteNonQuery();

                MessageBox.Show("Le montant a été ajouté à la facture du client", "success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                txtPrixCLient.Clear();
                txtPrixCLient.Focus();

                connection.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error:: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        /// <summary>
        /// c'est evenet verifier le tye des donnees saisir
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtPrix_KeyPress(object sender, KeyPressEventArgs e)
        {
            char comVer = Convert.ToChar(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
            if (double.TryParse(Clipboard.GetText(), out double vlr) && e.KeyChar == 22)
            {
                e.Handled = true;
            }

            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != comVer)
            {
                e.Handled = true;
            }

            if (e.KeyChar == comVer && (((sender as TextBox).Text.IndexOf(comVer) > -1) || (sender as TextBox).TextLength == 0))
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// verifier c'est la champs n'est pas vide
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtPrix_TextChanged(object sender, EventArgs e)
        {
            if (txtPrixCLient.TextLength > 0)
                btnConfirm.Enabled = true;
            else
                btnConfirm.Enabled = false;
        }

        /// <summary>
        /// c'est le button pour annuler le traitement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAnnuler_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
