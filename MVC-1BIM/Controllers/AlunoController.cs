using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using MVC_1BIM.Models;

using ClosedXML.Excel;

namespace MVC_1BIM.Controllers
{
    public class AlunoController : Controller
    {
        private static List<Aluno> alunos = new List<Aluno>
        {
            new Aluno { Id = 1, Nome = "Jones Henrique", RA = "12345", Email = "jonh@email.com", DataNascimento = new DateTime(2007, 6, 29) },
            new Aluno { Id = 2, Nome = "Richard Sandri", RA = "54321", Email = "richa@email.com", DataNascimento = new DateTime(2008, 8, 25) }
        };

        public IActionResult Index() => View(alunos);

        public IActionResult Cadastrar() => View();

        [HttpPost]
        public IActionResult Cadastrar(Aluno aluno)
        {
            aluno.Id = alunos.Max(a => a.Id) + 1;
            alunos.Add(aluno);
            return RedirectToAction("Index");
        }

        public IActionResult Editar(int id) => View(alunos.FirstOrDefault(a => a.Id == id));

        [HttpPost]
        public IActionResult Editar(Aluno aluno)
        {
            var existente = alunos.FirstOrDefault(a => a.Id == aluno.Id);
            if (existente != null)
            {
                existente.Nome = aluno.Nome;
                existente.RA = aluno.RA;
                existente.Email = aluno.Email;
                existente.DataNascimento = aluno.DataNascimento;
            }
            return RedirectToAction("Index");
        }

        public IActionResult Excluir(int id) => View(alunos.FirstOrDefault(a => a.Id == id));

        [HttpPost]
        public IActionResult Excluir(Aluno aluno)
        {
            alunos.RemoveAll(a => a.Id == aluno.Id);
            return RedirectToAction("Index");
        }

        public IActionResult Visualizar(int id) => View(alunos.FirstOrDefault(a => a.Id == id));

        public IActionResult GerarPdf()
        {
            var stream = new MemoryStream();
            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            // Fontes
            var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var regularFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            // Título
            var titulo = new Paragraph("📄 Lista de Alunos")
                .SetFont(boldFont)
                .SetFontSize(18)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20);
            document.Add(titulo);

            // Tabela com 5 colunas 
            Table tabela = new Table(5);
            tabela.SetWidth(UnitValue.CreatePercentValue(100));

            // Cabeçalho da tabela
            string[] headers = { "ID", "Nome", "RA", "Email", "Nascimento" };
            foreach (var header in headers)
            {
                tabela.AddHeaderCell(new Cell()
                    .Add(new Paragraph(header).SetFont(boldFont))
                    .SetBackgroundColor(iText.Kernel.Colors.ColorConstants.LIGHT_GRAY)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetPadding(5));
            }

            // Dados dos alunos
            foreach (var aluno in alunos)
            {
                tabela.AddCell(new Cell().Add(new Paragraph(aluno.Id.ToString())).SetFont(regularFont).SetPadding(5));
                tabela.AddCell(new Cell().Add(new Paragraph(aluno.Nome)).SetFont(regularFont).SetPadding(5));
                tabela.AddCell(new Cell().Add(new Paragraph(aluno.RA)).SetFont(regularFont).SetPadding(5));
                tabela.AddCell(new Cell().Add(new Paragraph(aluno.Email)).SetFont(regularFont).SetPadding(5));
                tabela.AddCell(new Cell().Add(new Paragraph(aluno.DataNascimento.ToString("dd/MM/yyyy"))).SetFont(regularFont).SetPadding(5));
            }

            document.Add(tabela);
            document.Close();

            return File(stream.ToArray(), "application/pdf", "alunos.pdf");
        }

        public IActionResult GerarExcel()
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Alunos");

                worksheet.Cell(1, 1).Value = "ID";
                worksheet.Cell(1, 2).Value = "Nome";
                worksheet.Cell(1, 3).Value = "RA";
                worksheet.Cell(1, 4).Value = "Data de Nascimento";

                var headerRange = worksheet.Range(1, 1, 1, 4);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                int linha = 2;
                foreach (var aluno in alunos)
                {
                    worksheet.Cell(linha, 1).Value = aluno.Id;
                    worksheet.Cell(linha, 2).Value = aluno.Nome;
                    worksheet.Cell(linha, 3).Value = aluno.RA;
                    worksheet.Cell(linha, 4).Value = aluno.DataNascimento.ToString("dd/MM/yyyy");
                    linha++;
                }

                worksheet.Columns().AdjustToContents();

                worksheet.Column(1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Column(3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Column(4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                var dataRange = worksheet.Range(1, 1, linha - 1, 4);
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ListaDeAlunos.xlsx");
                }
            }
        }


    }
}