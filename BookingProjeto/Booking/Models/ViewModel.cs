﻿using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Booking.Models
{
    public class ViewModel : PageModel
    {
        public ViewModel()
        {

        }

        public ViewModel(List<QuartosDisp> quartos, string message)
        {
            Quartos = quartos;
            Message = message;
        }

        public ViewModel(List<string> regimes, List<string> tipoPagamento, Dados dados, List<int> quantQuartos)
        {
            Regimes = regimes;
            TipoPagamento = tipoPagamento;
            Dados = dados;
            QuantQuartos = quantQuartos;
        }

        public string Message { get; private set; }

        public List<string> TipoPagamento { get; private set; }
        public List<string> Regimes { get; private set; }

        public List<int> QuantQuartos { get; set; }
        public Dados Dados { get; private set; }
        public List<string> Tipo { get; private set; }
        public List<QuartosDisp> Quartos { get; private set; }
    }
}
