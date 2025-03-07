using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Settings.Configuration;

namespace WebQueries.KTO.Models
{
    internal class CaseTypeSettingsRetriever
    {
        private readonly OmcConfiguration _configuration;

        public CaseTypeSettingsRetriever(OmcConfiguration configuration)
        {
            _configuration = configuration;
        }


    }
}
