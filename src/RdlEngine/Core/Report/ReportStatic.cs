using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.IO;

namespace Reporting.Rdl
{
    public partial class Report
    {
        public static string EmptyMessage()
        {
            string prog = "<Report><Width>8.5in</Width><Body><Height>1in</Height><ReportItems><Textbox><Value></Value><Style><FontWeight>Bold</FontWeight></Style><Height>.3in</Height><Width>5 in</Width></Textbox></ReportItems></Body></Report>";
            return prog;
        }

        public static string ErrorMessage(IList errorMessages)
        {
            string data1 = @"<?xml version='1.0' encoding='UTF-8'?>
<Report> 
	<LeftMargin>.4in</LeftMargin><Width>8.5in</Width>
	<Author></Author>
	<DataSources>
		<DataSource Name='DS1'>
			<ConnectionProperties> 
				<DataProvider>xxx</DataProvider>
				<ConnectString></ConnectString>
			</ConnectionProperties>
		</DataSource>
	</DataSources>
	<DataSets>
		<DataSet Name='Data'>
			<Query>
				<DataSourceName>DS1</DataSourceName>
			</Query>
			<Fields>
				<Field Name='Error'> 
					<DataField>Error</DataField>
					<TypeName>String</TypeName>
				</Field>
			</Fields>";

            string data2 = @"
		</DataSet>
	</DataSets>
	<PageHeader>
		<Height>1 in</Height>
		<ReportItems>
			<Textbox><Top>.1in</Top><Value>reportFU</Value><Style><FontSize>18pt</FontSize><FontWeight>Bold</FontWeight></Style></Textbox>
			<Textbox><Top>.1in</Top><Left>4.25in</Left><Value>=Globals!ExecutionTime</Value><Style><Format>dddd, MMMM dd, yyyy hh:mm:ss tt</Format><FontSize>12pt</FontSize><FontWeight>Bold</FontWeight></Style></Textbox>
			<Textbox><Top>.5in</Top><Value>Errors processing report</Value><Style><FontSize>12pt</FontSize><FontWeight>Bold</FontWeight></Style></Textbox>
		</ReportItems>
	</PageHeader>
	<Body><Height>3 in</Height>
		<ReportItems>
			<Table>
				<Style><BorderStyle>Solid</BorderStyle></Style>
				<TableColumns>
					<TableColumn><Width>7 in</Width></TableColumn>
				</TableColumns>
				<Header>
					<TableRows>
						<TableRow>
							<Height>15 pt</Height>
							<TableCells>
								<TableCell>
									<ReportItems><Textbox><Value>Messages</Value><Style><FontWeight>Bold</FontWeight></Style></Textbox></ReportItems>
								</TableCell>
							</TableCells>
						</TableRow>
					</TableRows>
					<RepeatOnNewPage>true</RepeatOnNewPage>
				</Header>
				<Details>
					<TableRows>
						<TableRow>
							<Height>12 pt</Height>
							<TableCells>
								<TableCell>
									<ReportItems><Textbox Name='ErrorMsg'><Value>=Fields!Error.Value</Value><CanGrow>true</CanGrow></Textbox></ReportItems>
								</TableCell>
							</TableCells>
						</TableRow>
					</TableRows>
				</Details>
			</Table>
		</ReportItems>
	</Body>
</Report>";

            StringBuilder sb = new StringBuilder(data1, data1.Length + data2.Length + 1000);
            // Build out the error messages
            sb.Append("<Rows>");
            foreach (string msg in errorMessages)
            {
                sb.Append("<Row><Error>");
                string newmsg = msg.Replace("&", @"&amp;");
                newmsg = newmsg.Replace("<", @"&lt;");
                sb.Append(newmsg);
                sb.Append("</Error></Row>");
            }
            sb.Append("</Rows>");
            sb.Append(data2);
            return sb.ToString();
        }

        public static string RdlSource(Uri reportPath)
        {
            StreamReader fs = null;
            string prog = null;
            try
            {
                fs = new StreamReader(reportPath.LocalPath);
                prog = fs.ReadToEnd();
            }
            finally
            {
                if (fs != null)
                    fs.Close();
            }

            return prog;
        }
    }
}
