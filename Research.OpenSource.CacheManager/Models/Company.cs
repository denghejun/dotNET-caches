using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Research.OpenSource.CacheManager.Models
{
    public class Company
    {
        public static string CACHE_KEY = "Companies";
        public static List<Company> MovieCompanies
        {
            get
            {
                return new List<Company>()
                {
                    new Company() {  CompanyName = "寰宇国际", CompanyType= CompanyType.MovieCompany},
                    new Company() {  CompanyName = "华谊兄弟", CompanyType= CompanyType.MovieCompany}
                };
            }
        }

        public static List<Company> ITCompanies
        {
            get
            {
                return new List<Company>()
                {
                    new Company() {  CompanyName = "Newegg", CompanyType = CompanyType.ITCompany},
                    new Company() {  CompanyName = "Baidu",CompanyType= CompanyType.ITCompany}
                };
            }
        }

        public string CompanyName { get; set; }
        public CompanyType CompanyType { get; set; }
    }

    public enum CompanyType
    {
        MovieCompany,
        ITCompany
    }
}
