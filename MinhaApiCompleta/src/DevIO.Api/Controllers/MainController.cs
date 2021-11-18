using DevIO.Business.Intefaces;
using DevIO.Business.Notificacoes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Linq;

namespace DevIO.Api.Controllers
{
    [ApiController]
    public abstract class MainController : ControllerBase
    {
        private readonly INotificador _notificador;

        protected MainController(INotificador notificador)
        {
            _notificador = notificador;
        }

        protected bool OperacaoValida()
        {
            return !_notificador.TemNotificacao(); //se o notifidicador não possuir nenhuma notificação
        }


        //valida se existe algum tipo de notificação
        protected ActionResult CustomResponse(object resultado = null)
        {
            if (OperacaoValida())
            {
                return Ok(new
                {
                    sucesso = true,
                    data = resultado
                });
            }

            return BadRequest(new { 
                sucesso = false,
                erros = _notificador.ObterNotificacoes().Select(m => m.Mensagem) //cria uma lista de mensagens de erro
            });
        }

        //sem validar a regra de negócio, verificando apenas com a modelState
        protected ActionResult CustomResponse(ModelStateDictionary modelState)
        {
            if (!modelState.IsValid) NotificarErroModelInvalida(modelState);
            return CustomResponse();
        }
        
        protected void NotificarErroModelInvalida(ModelStateDictionary modelState)
        {
            //selecionar os error
            var erros = modelState.Values.SelectMany(e => e.Errors);
            foreach(var erro in erros)
            {
                var errorMsg = erro.Exception == null ? erro.ErrorMessage : erro.Exception.Message;
                NotificarErro(errorMsg);
            }
        }

        protected void NotificarErro(string mensagem)
        {
            _notificador.Handle(new Notificacao(mensagem));
        }
    }
}

//validação de notificações de erro
//validação da Modelstate
//validação da operação de negócios