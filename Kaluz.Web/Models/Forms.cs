using System.ComponentModel.DataAnnotations;
namespace Kaluz.Web.Models;
public class LoginForm { [Required,EmailAddress] public string Email {get;set;}=""; [Required] public string Password {get;set;}=""; }
public class ChangePasswordForm { [Required] public string Actual {get;set;}=""; [Required,MinLength(8)] public string Nueva {get;set;}=""; }
public class SaleForm {public int? ClienteId {get;set;} public int MetodoPagoId {get;set;} [Range(0,100000)] public decimal Descuento {get;set;} public List<SaleLine> Lineas {get;set;}=new();}
public class SaleLine {[Range(1,int.MaxValue)] public int VarianteId {get;set;} [Range(1,10000)]public int Cantidad {get;set;} }
public class PurchaseForm {[Range(1,int.MaxValue)]public int ProveedorId {get;set;} public string? Documento {get;set;} public List<PurchaseLine> Lineas {get;set;}=new();}
public class PurchaseLine {public int VarianteId {get;set;} public int Cantidad {get;set;} public decimal Costo {get;set;} }
public class ReturnForm {public int VentaId {get;set;} public int DetalleVentaId {get;set;} public int Cantidad {get;set;} public string Tipo {get;set;}="Devolucion";public int? VarianteCambio {get;set;} public string? Motivo {get;set;} }
public class StockForm {public int VarianteId {get;set;} public int Diferencia {get;set;} public string Motivo {get;set;}="Ajuste manual";}
