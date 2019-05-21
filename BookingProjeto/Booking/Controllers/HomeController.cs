﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Booking.Models;
using Microsoft.AspNetCore.Authorization;
using Booking.Data;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Drawing;

namespace Booking.Controllers
{
    public class HomeController : Controller
    {
        BookingContext db = new BookingContext();

        //[HttpGet]
        //public IActionResult Index()
        //{
        //    return View();
        //}


        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }


        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }


        public IActionResult Privacy()
        {
            return View();
        }

        
        [HttpGet]
        public ActionResult Index(DateTime CheckIn, DateTime CheckOut, string tipoQuarto, int QuantQuartos)
        {
            //var cs = "Server=Ricki-PC; Database=Booking; Trusted_Connection=True;";
            var cs = "server=DESKTOP-IH74466; database=Booking; Trusted_Connection=True;";


            long TipoQuarto = BuscarIdtipoQuarto(tipoQuarto, cs);

            var result = CheckAvailability(cs, CheckIn, CheckOut, TipoQuarto, QuantQuartos);

            List<String> list2 = PreencheTipos(cs);

            var list = MostrarQuartos(cs, result, CheckIn, CheckOut);

            ViewModel model = new ViewModel(list, list2);

            return View(model);
        }



        public bool CheckAvailability(string cs, DateTime CheckIn, DateTime CheckOut, long TipoQuarto, int QuantQuartos)
        {
            int inventario = 0;
            int quantQuartos = 0;

            using (var cn = new SqlConnection(cs))
            {
                cn.Open();

                string sql = "select tq.IDTipoQuarto, tq.Inventario from TipoQuarto tq where tq.IdTipoQuarto = " + TipoQuarto;
                string sql2 = "select rs.IDTipoQuarto, rs.QuantQuartos from Reservas rs where rs.IdTipoQuarto = " + TipoQuarto;

                using (var cm = new SqlCommand(sql, cn))
                {
                    var rd = cm.ExecuteReader();

                    while (rd.Read())
                    {
                        inventario = rd.GetInt32(rd.GetOrdinal("Inventario"));
                    }
                    rd.Close();
                }

                using (var cm = new SqlCommand(sql2, cn))
                {
                    var rd = cm.ExecuteReader();

                    while (rd.Read())
                    {
                        quantQuartos = rd.GetInt32(rd.GetOrdinal("QuantQuartos"));
                    }
                    rd.Close();
                }

                for (DateTime i = CheckIn; i <= CheckOut; i.AddDays(1))
                {
                    if (inventario - quantQuartos >= QuantQuartos)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                cn.Close();
            }
            return true;
        }

        public List<QuartosDisp> MostrarQuartos(string cs, bool result, DateTime ci, DateTime co)
        {
            var list = new List<QuartosDisp>();
            var list2 = new List<QuartosDisp>();

            using (var cn = new SqlConnection(cs))
            {
                cn.Open();

                string sql = "Select tq.IDTipoQuarto, r.TipoRegime, tq.Imagem, tq.Descricao, tq.Capacidade, h.NomeHotel, h.NumEstrelas, h.Morada, h.Localidade, h.CodPostal, h.Pais, p.Preco " +
                             "From TipoQuarto tq, Hoteis h, Precario p, Regimes r " +
                             "Where tq.IDHotel = h.IDHotel and tq.IDTipoQuarto = p.IDTipoQuarto and r.IDRegime = p.IDRegime";

                string sql2 = "Select tq.IDTipoQuarto, h.IDHotel from TipoQuarto tq, Hoteis h where h.IDHotel = tq.IDHotel";

                if (result == true)
                {
                    using (var cm2 = new SqlCommand(sql2, cn))
                    {
                        var rd2 = cm2.ExecuteReader();

                        while (rd2.Read())
                        {
                            var id = new QuartosDisp();

                            id.IdTipoQuarto = rd2.GetInt64(rd2.GetOrdinal("IDTipoQuarto"));
                            id.IdHotel = rd2.GetInt64(rd2.GetOrdinal("IDHotel"));

                            list2.Add(id);    
                        }
                        rd2.Close();
                    }

                    using (var cm = new SqlCommand(sql, cn))
                    {
                        var rd = cm.ExecuteReader();

                        while (rd.Read())
                        {
                            var quartos = new QuartosDisp();

                            quartos.Imagem = rd.GetString(rd.GetOrdinal("Imagem"));
                            quartos.TipoQuarto = rd.GetString(rd.GetOrdinal("Descricao"));
                            quartos.Capacidade = rd.GetByte(rd.GetOrdinal("Capacidade"));
                            quartos.NomeHotel = rd.GetString(rd.GetOrdinal("NomeHotel"));
                            quartos.NumEstrelas = rd.GetString(rd.GetOrdinal("NumEstrelas"));
                            quartos.Morada = rd.GetString(rd.GetOrdinal("Morada"));
                            quartos.Localidade = rd.GetString(rd.GetOrdinal("Localidade"));
                            quartos.CodPostal = rd.GetString(rd.GetOrdinal("CodPostal"));
                            quartos.Pais = rd.GetString(rd.GetOrdinal("Pais"));
                            quartos.Preco = rd.GetDecimal(rd.GetOrdinal("Preco"));
                            quartos.IdTipoQuarto = rd.GetInt64(rd.GetOrdinal("IDTipoQuarto"));
                            quartos.CheckIn = ci;
                            quartos.CheckOut = co;

                            list.Add(quartos);
                            
                        }rd.Close();

                    }
                }

                for (int i = 0; i <= list2.Count() - 1; i++)
                {
                    for (int y = 1; y <= list.Count() - i - 1; y++)
                    {
                        if (i != y)
                        {
                            if (list2[i].IdTipoQuarto == list[y].IdTipoQuarto /*|| list2[i].IdHotel == list[y].IdHotel*/)
                            {
                                QuartosDisp outro = list[i];
                                list.Remove(outro);
                            }

                        }
                    }
                }
                
                cn.Close();
            }

            return list;
        }

        public long BuscarIdtipoQuarto(string tipoQuarto, string cs)
        {
            long TipoQuarto = 0;

            if (tipoQuarto != null)
            {
                using (var cn = new SqlConnection(cs))
                {
                    cn.Open();

                    string sql = "select tq.IDTipoQuarto from TipoQuarto tq where tq.Descricao = '" + tipoQuarto + "'";

                    using (var cm = new SqlCommand(sql, cn))
                    {
                        var rd = cm.ExecuteReader();

                        if (rd.Read())
                        {
                            TipoQuarto = rd.GetInt64(rd.GetOrdinal("IDTipoQuarto"));
                        }

                        rd.Close();
                    }
                    cn.Close();
                }
            }

            return TipoQuarto;
        }

        public List<String> PreencheTipos(string cs)
        {

            var list2 = new List<String>();
            var aux = new List<String>();

            using (var cn = new SqlConnection(cs))
            {
                cn.Open();

                string sql2 = "select tq.Descricao from TipoQuarto tq";

                using (var cm = new SqlCommand(sql2, cn))
                {
                    var rd = cm.ExecuteReader();

                    while (rd.Read())
                    {
                        string tipo = "";

                        tipo = rd.GetString(rd.GetOrdinal("Descricao"));

                        list2.Add(tipo);
                    }

                    aux = list2;

                    rd.Close();
                }
                cn.Close();
            }

            for (int i = 0; i <= list2.Count() - 1; i++)
            {
                for (int y = 0; y <= aux.Count() -i - 1; y++)
                {
                    if (i != y)
                    {
                        if (list2[i].Equals(aux[y]) )
                        {
                            string outro = aux[y];

                            aux.Remove(outro);
                        }
                    }
                }
            }
            return aux;
        }

        [HttpPost]
        public ActionResult Book(string checkOut, string checkIn, decimal preco, string tipo, string hotel, string nomeR, string sobrenomeR, int adultos, int criancas, int quant, string regime, string nome, string sobrenome, string email, string telefone, string endereco, string codPostal, string localidade, string cc, string dataNasc, string nomeTitular, string numCartao, string tipoCartao, string prazo, string cvv)
        {
            //var cs = "Server=Ricki-PC; Database=Booking; Trusted_Connection=True;";
            var cs = "server=DESKTOP-IH74466; database=Booking; Trusted_Connection=True;";

            AddCliente(cs, nome, sobrenome, email, telefone, endereco, codPostal, localidade, cc, dataNasc);

            AddPagamento(cs, nomeTitular, numCartao, tipoCartao, prazo, cvv);

            AddReserva(cs, nomeR, sobrenomeR, adultos, criancas, quant, regime, nome, cc, tipoCartao, hotel, tipo, checkOut, checkIn);

            return View();
        }

        public void AddReserva(string cs, string nomeR, string sobrenomeR, int adultos, int criancas, int quant, string regime, string nome, string cc, string tipoCartao, string hotel, string tipo, string checkOut, string checkIn)
        {
            int idHotel = 0;
            int idCliente = 0;
            int idRegime = 0;
            int idQuarto = 0;
            int idPag = 0;

            string sql = "Insert into Reservas values(@param1, @param2, @param3, @param4, @param5, @param6, @param7, @param8, @param9, @param10, @param11, @param12)";

            string sql2 = "select tp.IDTipoPagamento, tq.IDTipoQuarto, h.IDHotel, rg.IDRegime, cl.IDCliente from TipoPagamento tp, Hoteis h, TipoQuarto tq, Regimes rg, Clientes cl " +
                "where tq.IDHotel = h.IDHotel and rg.IDHotel = h.IDHotel and tp.Designacao = '" + tipoCartao + "' and h.NomeHotel = '" + hotel +  "' and " +
                "tq.Descricao = '" + tipo + "' and rg.TipoRegime = '" + regime + "' and cl.Nome = '" + nome + "' and cl.CC = '" + cc + "'";

            using (var cn = new SqlConnection(cs))
            {
                cn.Open();

                using (var cm = new SqlCommand(sql2, cn))
                {
                    var rd = cm.ExecuteReader();

                    while (rd.Read())
                    {
                        idPag = rd.GetInt16(rd.GetOrdinal("IDTipoPagamento"));
                        idQuarto = rd.GetInt16(rd.GetOrdinal("IDTipoQuarto"));
                        idHotel = rd.GetInt16(rd.GetOrdinal("IDTipoPagaIDHotelmento"));
                        idRegime = rd.GetInt16(rd.GetOrdinal("IDRegime"));
                        idCliente = rd.GetInt16(rd.GetOrdinal("IDCliente"));
                    }

                    rd.Close();
                }

                using (var cm = new SqlCommand(sql, cn))
                {
                    cm.Parameters.AddWithValue("@param1", idHotel);
                    cm.Parameters.AddWithValue("@param2", idCliente);
                    cm.Parameters.AddWithValue("@param3", idRegime);
                    cm.Parameters.AddWithValue("@param4", idQuarto);
                    cm.Parameters.AddWithValue("@param5", idPag);
                    cm.Parameters.AddWithValue("@param6", adultos);
                    cm.Parameters.AddWithValue("@param7", criancas);
                    cm.Parameters.AddWithValue("@param8", checkIn);
                    cm.Parameters.AddWithValue("@param9", checkOut);
                    cm.Parameters.AddWithValue("@param10", quant);
                    cm.Parameters.AddWithValue("@param11", nomeR);
                    cm.Parameters.AddWithValue("@param12", sobrenomeR);
                }
                cn.Close();
            }
        }

        public void AddPagamento(string cs, string nomeTitular, string numCartao, string tipoCartao, string prazo, string cvv)
        {
            int id = 0;
            string sql = "Insert into Pagamento values(@param1, @param2, @param3, @param4, @param5)";
            string sql2 = "Select tp.IDTipoPagamento from TipoPagamento tp where tp.Designacao = '" + tipoCartao + "'";

            using (var cn = new SqlConnection(cs))
            {
                cn.Open();

                using (var cm = new SqlCommand(sql2, cn))
                {
                    var rd = cm.ExecuteReader();

                    while (rd.Read())
                    {
                        id = rd.GetInt16(rd.GetOrdinal("IDTipoPagamento"));
                    }

                    rd.Close();
                }

                using (var cm = new SqlCommand(sql, cn))
                {
                    cm.Parameters.AddWithValue("@param1", id);
                    cm.Parameters.AddWithValue("@param2", nomeTitular);
                    cm.Parameters.AddWithValue("@param3", numCartao);
                    cm.Parameters.AddWithValue("@param4", prazo);
                    cm.Parameters.AddWithValue("@param5", cvv);
                }
                cn.Close();
            }
        }


        public void AddCliente(string cs, string nome, string sobrenome, string email, string telefone, string endereco, string codPostal, string localidade, string cc, string dataNasc)
        {
            string sql = "Insert into Clientes values(@param1, @param2, @param3, @param4, @param5, @param6, @param7, @param8, @param9)";

            using (var cn = new SqlConnection(cs))
            {
                cn.Open();

                using (var cm = new SqlCommand(sql, cn))
                {
                    cm.Parameters.AddWithValue("@param1", nome);
                    cm.Parameters.AddWithValue("@param2", sobrenome);
                    cm.Parameters.AddWithValue("@param3", email);
                    cm.Parameters.AddWithValue("@param4", telefone);
                    cm.Parameters.AddWithValue("@param5", endereco);
                    cm.Parameters.AddWithValue("@param6", localidade);
                    cm.Parameters.AddWithValue("@param7", codPostal);
                    cm.Parameters.AddWithValue("@param8", cc);
                    cm.Parameters.AddWithValue("@param9", dataNasc);
                }
                cn.Close();
            }
        }

        
        public ActionResult Book(string hotel, string quarto, decimal preco, DateTime CheckIn, DateTime CheckOut )
        {
            //var cs = "Server=Ricki-PC; Database=Booking; Trusted_Connection=True;";
            var cs = "server=DESKTOP-IH74466; database=Booking; Trusted_Connection=True;";

            Dados dados = new Dados(hotel, quarto, preco, CheckIn, CheckOut);

            List<string> list = PreencheRegimes(cs, hotel, quarto);
            List<string> list2 = PreencheTipoPagamento(cs);

            ViewModel model = new ViewModel(list, list2, dados);

            return View(model);
        }

        public List<string> PreencheTipoPagamento(string cs)
        {
            List<string> list = new List<string>();

            using (var cn = new SqlConnection(cs))
            {
                cn.Open();

                string sql = "select tp.Designacao from TipoPagamento tp";

                using (var cm = new SqlCommand(sql, cn))
                {
                    var rd = cm.ExecuteReader();

                    while (rd.Read())
                    {
                        string tipo = "";

                        tipo = rd.GetString(rd.GetOrdinal("Designacao"));

                        list.Add(tipo);
                    }
                    rd.Close();
                }
                cn.Close();
            }

            return list;
        }

        public List<string> PreencheRegimes(string cs, string hotel, string quarto)
        {
            List<string> list = new List<string>();

            using (var cn = new SqlConnection(cs))
            {
                cn.Open();

                string sql = "select rg.TipoRegime from Regimes rg, Hoteis h, TipoQuarto tq " +
                             "where rg.IDHotel = h.IDHotel and h.IDHotel = tq.IDHotel and h.NomeHotel = '" + hotel + "' and tq.Descricao = '" + quarto + "'";

                using (var cm = new SqlCommand(sql, cn))
                {
                    var rd = cm.ExecuteReader();

                    while (rd.Read())
                    {
                        string tipo = "";

                        tipo = rd.GetString(rd.GetOrdinal("TipoRegime"));

                        list.Add(tipo);
                    }
                    rd.Close();
                }
                cn.Close();
            }

            return list;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }




        //PARA MOSTRAR OS HOTEIS
        /*var list2 = new List<Hoteis>();
        string sql2 = "select NomeHotel, NumEstrelas, Morada, Localidade, CodPostal, Pais, QuantidadeQuartos, Descricao, Imagem from Hoteis ";

         using (var cm = new SqlCommand(sql2, cn))
                {
                    var rd = cm.ExecuteReader();

                    while (rd.Read())
                    {
                        var hoteis = new Hoteis();
                        
                        hoteis.NomeHotel = rd.GetString(rd.GetOrdinal("NomeHotel"));
                        hoteis.NumEstrelas = rd.GetString(rd.GetOrdinal("NumEstrelas"));
                        hoteis.Morada = rd.GetString(rd.GetOrdinal("Morada"));
                        hoteis.Localidade = rd.GetString(rd.GetOrdinal("Localidade"));
                        hoteis.CodPostal = rd.GetString(rd.GetOrdinal("CodPostal"));
                        hoteis.Pais = rd.GetString(rd.GetOrdinal("Pais"));
                        hoteis.QuantidadeQuartos = rd.GetInt16(rd.GetOrdinal("QuantidadeQuartos"));
                        hoteis.Descricao = rd.GetString(rd.GetOrdinal("Descricao"));
                        hoteis.Imagem = rd.GetString(rd.GetOrdinal("Imagem"));

                        list2.Add(hoteis);
                    }
                    rd.Close();
                }
        */


        //PARA AS ESTRELAS

        /*if (quartos.NumEstrelas.Equals("1"))
        {
            string html = "<p><i class='fa fa-star' style='font-size:24px; color:yellow'></i></p>";
        }
        else
        if (quartos.NumEstrelas.Equals("2"))
        {
            string html2 = "< i class='fa fa-star' style='font-size:24px; color:yellow'></i>" +
                "<i class='fa fa-star' style='font-size:24px; color:yellow'></i>";
        }
        else
        if (quartos.NumEstrelas.Equals("3"))
        {
            string html3 = "<p><i class='fa fa-star' style='font-size: 24px; color:yellow' ></i>" +
                "<i class='fa fa-star' style ='font -size:24px; color:yellow' ></i>" +
                "<i class='fa fa-star' style ='font -size:24px; color:yellow' ></i></p>";
        }
        else
        if (quartos.NumEstrelas.Equals("4"))
        {
            string html4 = "<i class='fa fa-star' style='font-size:24px; color:yellow'></i>" +
                            "<i class='fa fa-star' style ='font -size:24px; color:yellow' ></i>" +
                            "<i class='fa fa-star' style ='font -size:24px; color:yellow' ></i>" +
                            "<i class='fa fa-star' style ='font -size:24px; color:yellow' ></i>";
        }
        else
        if (quartos.NumEstrelas.Equals("5"))
        {
            string html5 = "<i class='fa fa-star' style='font-size:24px; color:yellow'></i>" +
                            "<i class='fa fa-star' style='font-size:24px; color:yellow'></i>" +
                            "<i class='fa fa-star' style='font-size:24px; color:yellow'></i>" +
                            "<i class='fa fa-star' style='font-size:24px; color:yellow'></i>" +
                            "<i class='fa fa-star' style='font-size:24px; color:yellow'></i>";
        } */


        //1 tabela
        /*string sql = "select r.CheckIn as FromDate, r.CheckOut as ToDate, h.NomeHotel as HotelName, tq.Descricao as RoomCategory, p.Preco as Price " +
                        "from Reservas r, Hoteis h, TipoQuarto tq, Precario p " +
                        "where r.IDHotel = h.IDHotel and r.IDTipoQuarto = tq.IDTipoQuarto and p.IDTipoQuarto = tq.IDTipoQuarto";

        using (var cm = new SqlCommand(sql, cn))
        {
            var rd = cm.ExecuteReader();
            while (rd.Read())
            {
                var o = new Ofertas();

                o.FromDate = rd.GetDateTime(rd.GetOrdinal("FromDate"));
                o.ToDate = rd.GetDateTime(rd.GetOrdinal("toDate"));
                o.HotelName = rd.GetString(rd.GetOrdinal("HotelName"));
                o.RoomCategory = rd.GetString(rd.GetOrdinal("RoomCategory"));
                o.Price = rd.GetDecimal(rd.GetOrdinal("Price"));

                list.Add(o);
            }
        }*/

    }
}
