using System;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace sistema_gastronomico_pascual_leyes_delahoz_clavero.Models
{
    public abstract class RepositorioBase
    {
        protected readonly IConfiguration configuration;
        protected readonly String connectionString;

          protected RepositorioBase(IConfiguration configuration)
        {
            this.configuration = configuration;
            this.connectionString = configuration.GetConnectionString("MySql");
        }
    }
}