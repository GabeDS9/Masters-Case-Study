﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Precinct_DT
{
    class PrecinctManager
    {
        public List<Precinct> InitialisePrecincts(string campus_name, string startingDate)
        {
            LoadExcel excel = new LoadExcel();
            List<Precinct> precinctList = new List<Precinct>();
            precinctList = excel.LoadPrecinctData();
            return precinctList;
        }
    }
}