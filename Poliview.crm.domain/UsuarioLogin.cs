using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Poliview.crm.domain
{
    public class UsuarioLogin
    {
        [Display(Name = "Usuario"), Required(ErrorMessage = "Campo Usuário é obrigatório")]
        public string Email { get; set; }
        [Display(Name = "Senha"), Required(ErrorMessage = "Campo Senha é obrigatório")]
        public string Senha { get; set; }
    }
}
