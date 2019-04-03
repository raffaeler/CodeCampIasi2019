using ModelLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace AuthorizationLibrary.Common
{
    public static class MaturityHelper
    {
        public static Maturity GetMaturity(ClaimsPrincipal user)
        {
            var birth = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.DateOfBirth);
            if (birth == null)
            {
                return Maturity.Unclassified;
            }

            // obtained with 
            // datetime.ToString("s", System.Globalization.CultureInfo.InvariantCulture);
            var birthDate = DateTimeOffset.Parse(birth.Value);
            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;
            if (birthDate > today.AddYears(-age)) age--;

            // https://ieeexplore.ieee.org/document/6416855/
            if (age < 13) return Maturity.Child;
            if (age < 19) return Maturity.Adolescent;
            if (age < 60) return Maturity.Adult;
            return Maturity.Senior;
        }

    }
}
