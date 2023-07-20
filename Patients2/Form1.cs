using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using DevExpress.XtraEditors.Filtering;
using DevExpress.XtraTreeList.Columns;
using DevExpress.XtraTreeList;
using DevExpress.XtraTreeList.Nodes;
using FellowOakDicom;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static Patients2.treeList;
using DevExpress.Data.TreeList;
using System.Security.Cryptography;
using static DevExpress.Utils.Svg.CommonSvgImages;
using Microsoft.Extensions.Primitives;
using DevExpress.XtraPrinting.Native;

namespace Patients2
{
    public partial class treeList : Form
    {
        public treeList()
        {
            InitializeComponent();
        }
        

        private void browseButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
           

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string fileName = openFileDialog1.FileName;
                filePath.Text = fileName;
            }
         
        }

        //open button
        private void searchButton_Click(object sender, EventArgs e)
        {
            string dicomFilePath = filePath.Text;

            treeList1.Nodes.Clear();
            treeList1.Columns.Clear();

            DicomDataset dataset = DicomFile.Open(dicomFilePath).Dataset; //ca sa nu crape la recursivitate

            CreateColumns(treeList1);

            CreateNodes(treeList1, dataset, null); //null e root by default pt toate rooturile 


        }

        //pt headere de coloane si creaza coloane
        private void CreateColumns(TreeList tl)
        {
            tl.BeginUpdate();
            TreeListColumn col1 = tl.Columns.Add();
            col1.FieldName = "Tag"; //numele fieldului, ca variabila din clasa
            col1.Caption = "Tag ID"; //ce apare pe header
            col1.VisibleIndex = 0; //nr coloana
            TreeListColumn col2 = tl.Columns.Add();
            col2.FieldName = "VR";
            col2.Caption = "VR";
            col2.VisibleIndex = 1;
            TreeListColumn col3 = tl.Columns.Add();
            col3.FieldName = "Description";
            col3.Caption = "Description";
            col3.VisibleIndex = 2;
            TreeListColumn col4 = tl.Columns.Add();
            col4.FieldName = "Value";
            col4.Caption = "Value";
            col4.VisibleIndex = 3;
            tl.EndUpdate();
        }


        public class DicomData
        {
            public string Tag { get; set; }
            public string VR { get; set; }
            public string Description { get; set; }
            public string Value { get; set; }
            public bool isModified { get; set; }
            public DicomData(string _Tag, string _VR, string _Description, string _Value, bool _isModified) 
            {
                Tag = _Tag; VR = _VR;   Description = _Description; Value = _Value;
                isModified = _isModified;
            }
        }

        //treeList = locul unde afisezi 
        //dataset = ramane la fel = datele din fisier 
        //rootNodes =  rootul pt fiecare secventa, se modifica pe parcus cum gasesti seq noua 
        private void CreateNodes(TreeList treeList, DicomDataset dataset, TreeListNode rootNodes)
        {
            treeList.BeginUnboundLoad();

            foreach (DicomItem item in dataset)
            {

                //iei datele din fisier 
                string Tag = item.Tag.ToString();
                var VR = item.ValueRepresentation.ToString();

                string originalString = item.ToString(); //aici calculez descrierea 
                int startIndex = 15;
                string Description = originalString.Substring(startIndex);


                //daca gasesti secventa retii rootul si parcurgi seq 
                if (VR == "SQ")
                {
                    string Value = string.Empty; //rootul de seq nu are valoare 

                    //creezi secventa 
                    //var sequenceRoot = treeList.AppendNode(new object[] { Tag,VR,Description,Value}, rootNodes);
                   
                    TreeListNode sequenceRoot = treeList1.AppendNode(new object[] { Tag, VR, Description, Value }, rootNodes);
                    DicomData data = new DicomData(Tag, VR, Description, Value, false);
                    sequenceRoot.Tag = data;// asta imi leaga obiectul de reprezentarea prin stringuri a nodului in TreeListView
                    foreach (DicomDataset seqItem in ((DicomSequence)item).Items)
                    {
                        CreateNodes(treeList, seqItem, sequenceRoot); //creezi nodul pe secventa 
                    }
                }
                
                //cand nu e secventa sau un tip special de VR
                else if (VR != "UN" && VR != "SQ")
                {
                    string Value = dataset.GetString(item.Tag);
                    //treeList.AppendNode(new object[] { Tag, VR, Description, Value }, rootNodes);//adaugi nodul la lista ca root basic
                    
                    TreeListNode newNode = treeList1.AppendNode(new object[] { Tag, VR, Description, Value }, rootNodes);
                    DicomData data = new DicomData(Tag, VR, Description, Value, false);
                    newNode.Tag = data;// asta imi leaga obiectul de reprezentarea prin stringuri a nodului in TreeListView
                    
                }

            }
            treeList.EndUnboundLoad();
        }


        private void filePath_TextChanged(object sender, EventArgs e)
        {

        }

        //DevExpress.XtraTreeList.
        private void treeList1_FocusedNodeChanged(object sender, FocusedNodeChangedEventArgs e)
        {
 
        }

        //CAUTA IN TIMP CE SCRIU 
        //trb sa fac si pt child nodes !!!!!!!!!!
        private void textSearchBox_TextChanged(object sender, EventArgs e)
        {
           
            foreach (TreeListNode parentNode in treeList1.Nodes)
            {
                parentNode.Visible = true;
            }
            string filterText = textSearchBox.Text;


            foreach (TreeListNode parentNode in treeList1.Nodes)
            {
                if (NodeContainsText(parentNode, filterText) == false)
                {
                    parentNode.Visible = false;
                }
            }

        }

        //DACA VREAU SA MEARGA PE BUTON DE FIND
        //private void findButton_Click(object sender, EventArgs e)
        //{
        //    foreach (TreeListNode parentNode in treeList1.Nodes)
        //    {
        //        parentNode.Visible = true;
        //    }
        //    string filterText = textSearchBox.Text;
        //    foreach (TreeListNode parentNode in treeList1.Nodes)
        //    {
        //        if (NodeContainsText(parentNode, filterText) == false)
        //        {
        //            parentNode.Visible = false;
        //        }
        //    }
        //}

        private bool NodeContainsText(TreeListNode node, string searchText)
        {

            // string nodeTxt =node.ToString();    asta nu e bun
            string completeNodeText = "";

            // Iterate through each column in the TreeList
            foreach (TreeListColumn column in treeList1.Columns)
            {
                // Get the display text of the node for the current column
                string columnText = node.GetDisplayText(column);

                // Concatenate the display text with the completeNodeText
                completeNodeText += columnText + " ";
            }

            // Trim any extra whitespace from the end of the completeNodeText
            completeNodeText = completeNodeText.Trim();

            return completeNodeText.Contains(searchText);
        }



        private void saveButton_Click(object sender, EventArgs e)
        {
            // Assuming you have a List<TreeListNode> containing the nodes
            List<TreeListNode> nodes = new List<TreeListNode>(treeList1.Nodes);

            // Step 1: Create a new DicomDataset and add the modified DicomElements to it
            DicomDataset modifiedDataset = new DicomDataset();
            foreach (var node in nodes)
            {
                if (node.Tag != null && node.Tag is DicomData dicomData && dicomData.isModified)
                {
                    // Add or update the DICOM element in the modified dataset
                    DicomData data = (DicomData)node.Tag;
                    DicomTag dtag = DicomTag.Parse(data.Tag);
                    DicomVR dvr = DicomVR.Parse(data.VR);
                    string[] dval = { data.Value };

                    modifiedDataset.AddOrUpdate<string>(dvr, dtag, dval);

                    // Reset the IsModified flag after saving the data
                    dicomData.isModified = false;
                }
                //else
                //{
                //    modifiedDataset.Add(data);
                //    DicomData data = (DicomData)node.Tag;
                //    DicomTag dtag = DicomTag.Parse(data.Tag);
                //    DicomVR dvr = DicomVR.Parse(data.VR);
                //    string[] dval = { data.Value };

                //    //modifiedDataset.AddOrUpdate<string>(dvr, dtag, dval);
                //}
            }

            // Step 2: Save the modified DicomDataset as a new DICOM file
            string newDicomFilePath = "./path_to_save_new_dicom_file.dcm";
            DicomFile newDicomFile = new DicomFile(modifiedDataset);
            newDicomFile.Save(newDicomFilePath);
        }




        private void treeList1_NodeChanged(object sender, NodeChangedEventArgs e)
        {
            
        }

        private void treeList1_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            TreeList treeList = (TreeList)sender;
            TreeListNode modifiedNode = e.Node;

            // Retrieve the associated DicomData instance from the Tag property of the modified node
            if (modifiedNode.Tag != null && modifiedNode.Tag is DicomData dicomData)
            {
                // Update the DicomData properties based on the modified cell
                if (e.Column.FieldName == "Description")
                {
                    dicomData.Description = e.Value.ToString();
                }
                else if (e.Column.FieldName == "Value")
                {
                    dicomData.Value = e.Value.ToString();
                }

                // Mark the DicomData instance as modified
                dicomData.isModified = true;
            }
        }
    }
}






