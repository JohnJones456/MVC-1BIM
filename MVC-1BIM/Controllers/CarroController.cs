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
using System;

using ClosedXML.Excel;

namespace MVC_1BIM.Controllers
{
    public class CarroController : Controller
    {
        private static List<Carro> carros = new List<Carro>
        {
            new Carro { Id = 1, Marca = "Toyota", Modelo = "Corolla", Placa = "ABC1234", DataFabricacao = new DateTime(2020, 5, 10) },
            new Carro { Id = 2, Marca = "Ford", Modelo = "Fiesta", Placa = "XYZ5678", DataFabricacao = new DateTime(2019, 11, 23) }
        };

        public IActionResult Index() => View(carros);

        public IActionResult Cadastrar() => View();

        [HttpPost]
        public IActionResult Cadastrar(Carro carro)
        {
            carro.Id = carros.Any() ? carros.Max(c => c.Id) + 1 : 1;
            carros.Add(carro);
            return RedirectToAction("Index");
        }

        public IActionResult Editar(int id) => View(carros.FirstOrDefault(c => c.Id == id));

        [HttpPost]
        public IActionResult Editar(Carro carro)
        {
            var existente = carros.FirstOrDefault(c => c.Id == carro.Id);
            if (existente != null)
            {
                existente.Marca = carro.Marca;
                existente.Modelo = carro.Modelo;
                existente.Placa = carro.Placa;
                existente.DataFabricacao = carro.DataFabricacao;
            }
            return RedirectToAction("Index");
        }

        public IActionResult Excluir(int id) => View(carros.FirstOrDefault(c => c.Id == id));

        [HttpPost]
        public IActionResult Excluir(Carro carro)
        {
            carros.RemoveAll(c => c.Id == carro.Id);
            return RedirectToAction("Index");
        }

        public IActionResult Visualizar(int id) => View(carros.FirstOrDefault(c => c.Id == id));

        public IActionResult GerarPdf()
        {
            var stream = new MemoryStream();
            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var regularFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            var titulo = new Paragraph("🚗 Lista de Carros")
                .SetFont(boldFont)
                .SetFontSize(18)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20);
            document.Add(titulo);

            Table tabela = new Table(5);
            tabela.SetWidth(UnitValue.CreatePercentValue(100));

            string[] headers = { "ID", "Marca", "Modelo", "Placa", "Data Fabricação" };
            foreach (var header in headers)
            {
                tabela.AddHeaderCell(new Cell()
                    .Add(new Paragraph(header).SetFont(boldFont))
                    .SetBackgroundColor(iText.Kernel.Colors.ColorConstants.LIGHT_GRAY)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetPadding(5));
            }

            foreach (var carro in carros)
            {
                tabela.AddCell(new Cell().Add(new Paragraph(carro.Id.ToString())).SetFont(regularFont).SetPadding(5));
                tabela.AddCell(new Cell().Add(new Paragraph(carro.Marca)).SetFont(regularFont).SetPadding(5));
                tabela.AddCell(new Cell().Add(new Paragraph(carro.Modelo)).SetFont(regularFont).SetPadding(5));
                tabela.AddCell(new Cell().Add(new Paragraph(carro.Placa)).SetFont(regularFont).SetPadding(5));
                tabela.AddCell(new Cell().Add(new Paragraph(carro.DataFabricacao.ToString("dd/MM/yyyy"))).SetFont(regularFont).SetPadding(5));
            }

            document.Add(tabela);
            document.Close();

            return File(stream.ToArray(), "application/pdf", "carros.pdf");
        }

        public IActionResult GerarExcel()
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Carros");

                // Cabeçalho
                worksheet.Cell(1, 1).Value = "ID";
                worksheet.Cell(1, 2).Value = "Marca";
                worksheet.Cell(1, 3).Value = "Modelo";
                worksheet.Cell(1, 4).Value = "Placa";
                worksheet.Cell(1, 5).Value = "Data de Fabricação";

                // Estilo do cabeçalho
                var headerRange = worksheet.Range(1, 1, 1, 5);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                // Dados
                int linha = 2;
                foreach (var carro in carros)
                {
                    worksheet.Cell(linha, 1).Value = carro.Id;
                    worksheet.Cell(linha, 2).Value = carro.Marca;
                    worksheet.Cell(linha, 3).Value = carro.Modelo;
                    worksheet.Cell(linha, 4).Value = carro.Placa;
                    worksheet.Cell(linha, 5).Value = carro.DataFabricacao.ToString("dd/MM/yyyy");
                    linha++;
                }

                worksheet.Columns().AdjustToContents();

                // Alinhamento das colunas
                worksheet.Column(1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Column(4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Column(5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Bordas para os dados
                var dataRange = worksheet.Range(1, 1, linha - 1, 5);
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                // Exporta como arquivo
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ListaDeCarros.xlsx");
                }
            }
        }

    }
}
