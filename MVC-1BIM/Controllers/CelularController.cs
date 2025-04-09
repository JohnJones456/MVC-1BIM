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

namespace MVC_1BIM.Controllers
{
    public class CelularController : Controller
    {
        private static List<Celular> celulares = new List<Celular>
        {
            new Celular { Id = 1, Marca = "Samsung", Modelo = "Galaxy S21", Preco = 3500.00m, DataFabricacao = new DateTime(2021, 3, 15) },
            new Celular { Id = 2, Marca = "Apple", Modelo = "iPhone 13", Preco = 5000.00m, DataFabricacao = new DateTime(2022, 1, 10) }
        };

        public IActionResult Index() => View(celulares);

        public IActionResult Cadastrar() => View();

        [HttpPost]
        public IActionResult Cadastrar(Celular celular)
        {
            celular.Id = celulares.Any() ? celulares.Max(c => c.Id) + 1 : 1;
            celulares.Add(celular);
            return RedirectToAction("Index");
        }

        public IActionResult Editar(int id) => View(celulares.FirstOrDefault(c => c.Id == id));

        [HttpPost]
        public IActionResult Editar(Celular celular)
        {
            var existente = celulares.FirstOrDefault(c => c.Id == celular.Id);
            if (existente != null)
            {
                existente.Marca = celular.Marca;
                existente.Modelo = celular.Modelo;
                existente.Preco = celular.Preco;
                existente.DataFabricacao = celular.DataFabricacao;
            }
            return RedirectToAction("Index");
        }

        public IActionResult Excluir(int id) => View(celulares.FirstOrDefault(c => c.Id == id));

        [HttpPost]
        public IActionResult Excluir(Celular celular)
        {
            celulares.RemoveAll(c => c.Id == celular.Id);
            return RedirectToAction("Index");
        }

        public IActionResult Visualizar(int id) => View(celulares.FirstOrDefault(c => c.Id == id));

        public IActionResult GerarPdf()
        {
            var stream = new MemoryStream();
            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var regularFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            var titulo = new Paragraph("📱 Lista de Celulares")
                .SetFont(boldFont)
                .SetFontSize(18)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20);
            document.Add(titulo);

            Table tabela = new Table(5);
            tabela.SetWidth(UnitValue.CreatePercentValue(100));

            string[] headers = { "ID", "Marca", "Modelo", "Preço", "Data Fabricação" };
            foreach (var header in headers)
            {
                tabela.AddHeaderCell(new Cell()
                    .Add(new Paragraph(header).SetFont(boldFont))
                    .SetBackgroundColor(iText.Kernel.Colors.ColorConstants.LIGHT_GRAY)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetPadding(5));
            }

            foreach (var celular in celulares)
            {
                tabela.AddCell(new Cell().Add(new Paragraph(celular.Id.ToString())).SetFont(regularFont).SetPadding(5));
                tabela.AddCell(new Cell().Add(new Paragraph(celular.Marca)).SetFont(regularFont).SetPadding(5));
                tabela.AddCell(new Cell().Add(new Paragraph(celular.Modelo)).SetFont(regularFont).SetPadding(5));
                tabela.AddCell(new Cell().Add(new Paragraph("R$ " + celular.Preco.ToString("F2"))).SetFont(regularFont).SetPadding(5));
                tabela.AddCell(new Cell().Add(new Paragraph(celular.DataFabricacao.ToString("dd/MM/yyyy"))).SetFont(regularFont).SetPadding(5));
            }

            document.Add(tabela);
            document.Close();

            return File(stream.ToArray(), "application/pdf", "celulares.pdf");
        }
    }
}
