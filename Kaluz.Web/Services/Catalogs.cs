namespace Kaluz.Web.Services;
public record FieldDef(string Name,string Label,string Type="text",bool Required=false);
public record EntityDef(string Table,string Key,string Caption,FieldDef[] Fields);
public static class Catalogs {
 public static readonly Dictionary<string,EntityDef> All = new(StringComparer.OrdinalIgnoreCase){
 ["Categorias"]=new("Categoria","IdCategoria","Categorías",[new("Nombre","Nombre","text",true),new("Activo","Activo","bool")]),
 ["Productos"]=new("Producto","IdProducto","Productos",[new("Codigo","Código","text",true),new("Nombre","Nombre","text",true),new("Descripcion","Descripción"),new("IdCategoria","ID categoría","number",true),new("Marca","Marca"),new("PrecioCosto","Precio costo","decimal",true),new("PrecioVenta","Precio venta","decimal",true),new("Activo","Activo","bool")]),
 ["Variantes"]=new("ProductoVariante","IdVariante","Variantes",[new("IdProducto","ID producto","number",true),new("Talla","Talla","text",true),new("Color","Color","text",true),new("SKU","SKU","text",true),new("StockMinimo","Stock mínimo","number",true),new("Activo","Activo","bool")]),
 ["Clientes"]=new("Cliente","IdCliente","Clientes",[new("Nombres","Nombres","text",true),new("Apellidos","Apellidos"),new("DNI","DNI"),new("Telefono","Teléfono"),new("Email","Email","email"),new("Notas","Notas"),new("Activo","Activo","bool")]),
 ["Proveedores"]=new("Proveedor","IdProveedor","Proveedores",[new("Nombre","Nombre","text",true),new("RUC","RUC"),new("Contacto","Contacto"),new("Telefono","Teléfono"),new("Email","Email","email"),new("Direccion","Dirección"),new("Activo","Activo","bool")]),
 ["CategoriasGasto"]=new("CategoriaGasto","IdCategoriaGasto","Categorías de gasto",[new("Nombre","Nombre","text",true)]),
 ["Gastos"]=new("Gasto","IdGasto","Gastos",[new("IdCategoriaGasto","ID categoría","number",true),new("Concepto","Concepto","text",true),new("Monto","Monto","decimal",true)]),
 ["Usuarios"]=new("Usuario","IdUsuario","Usuarios",[new("NombreCompleto","Nombre completo","text",true),new("Email","Email","email",true),new("IdRol","ID rol","number",true),new("Activo","Activo","bool")])
 };
}
