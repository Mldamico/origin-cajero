using System.Data.SqlClient;
using Dapper;

namespace origin.Data;

public class SeedData
{
    private readonly string connectionString; 
    public SeedData(string connString)
    {
        connectionString = connString;
    }
    
    public async Task Initialize()
    {
        using var connection = new SqlConnection(connectionString);
        
     
      
        await connection.ExecuteAsync(@"if not exists (select * from sysobjects where name='tipo_operacion' and xtype='U')
            CREATE TABLE 
            tipo_operacion (
            id INT IDENTITY (1, 1) NOT NULL,
            detalle NVARCHAR (50) NOT NULL,
            CONSTRAINT PK_tipo_operacion PRIMARY KEY CLUSTERED (id ASC)
        );");
        
        await connection.ExecuteAsync(@"if not exists (select * from sysobjects where name='cuenta' and xtype='U')
            CREATE TABLE cuenta (
            id  INT IDENTITY (1, 1) NOT NULL,
            pin NVARCHAR (10) NOT NULL,
            saldo DECIMAL (18, 2) NOT NULL,
            CONSTRAINT PK_Cuenta PRIMARY KEY CLUSTERED (id ASC)
        );");

        await connection.ExecuteAsync(@"if not exists (select * from sysobjects where name='tarjeta' and xtype='U')
            CREATE TABLE tarjeta (
               id                 INT           IDENTITY (1, 1) NOT NULL,
               numero            NVARCHAR (16) NOT NULL,
               bloqueada          BIT           CONSTRAINT DEFAULT_tarjeta_bloqueada DEFAULT 0 NOT NULL,
               intentos_restantes INT           CONSTRAINT DEFAULT_tarjeta_intentos_restantes DEFAULT 4 NOT NULL,
               cuenta_id          INT           NOT NULL,
               CONSTRAINT PK_tarjeta PRIMARY KEY CLUSTERED (id ASC),
               CONSTRAINT FK_tarjeta_cuenta FOREIGN KEY (cuenta_id) REFERENCES cuenta (id)
        );");


        await connection.ExecuteAsync(@"if not exists (select * from sysobjects where name='operacion' and xtype='U')
            CREATE TABLE operacion (
               id         INT             IDENTITY (1, 1) NOT NULL,
               fecha       DATETIME        NOT NULL,
               monto        DECIMAL (18, 2) NULL,
               tipo_operacion_id INT             NOT NULL,
               tarjeta_id   INT             NOT NULL,
               CONSTRAINT PK_operacion PRIMARY KEY CLUSTERED (id ASC),
               CONSTRAINT FK_operacion_tarjeta FOREIGN KEY (tarjeta_id) REFERENCES tarjeta (id),
               CONSTRAINT FK_operacion_tipo_operacion FOREIGN KEY (tipo_operacion_id) REFERENCES tipo_operacion (id)
        );");

        

    }

    public async Task Seed()
    {
        using var connection = new SqlConnection(connectionString);
        var tipoOperacionData = await connection.QueryFirstOrDefaultAsync<int>(@"select * from tipo_operacion");
        
        if (tipoOperacionData == 0)
        {
             await connection.ExecuteAsync(@"INSERT INTO tipo_operacion (detalle) values(@detalle);", new {detalle = "Balance"});
             await connection.ExecuteAsync(@"INSERT INTO tipo_operacion (detalle) values(@detalle);", new {detalle = "Retiro"});    
        }
        
        var cuentasData = await connection.QueryFirstOrDefaultAsync<int>(@"select * from cuenta");
        
        if (cuentasData == 0)
        {
            await connection.ExecuteAsync(@"INSERT INTO cuenta (pin, saldo) values(@pin, @saldo);", new {pin = "1234", saldo = 5000.00});
            await connection.ExecuteAsync(@"INSERT INTO cuenta (pin, saldo) values(@pin, @saldo);", new {pin = "9876", saldo = 10000.00});    
        }
        
        var tarjetasData = await connection.QueryFirstOrDefaultAsync<int>(@"select * from tarjeta");
        
        if (tarjetasData == 0)
        {
            await connection.ExecuteAsync(@"INSERT INTO tarjeta (cuenta_id, numero) values(@cuenta_id, @numero);", new {cuenta_id = 1, numero="4111111111111111"});
            await connection.ExecuteAsync(@"INSERT INTO tarjeta (cuenta_id, numero) values(@cuenta_id, @numero);", new {cuenta_id = 2, numero="4111111111111111"});    
        }
        
        var operacionesData = await connection.QueryFirstOrDefaultAsync<int>(@"select * from operacion");
        if (operacionesData == 0)
        {
            await connection.ExecuteAsync(@"INSERT INTO operacion (fecha, monto, tipo_operacion_id, tarjeta_id) values(@fecha, @monto, @tipo_operacion_id, @tarjeta_id);", new {fecha = DateTime.Today, monto=100.00, tipo_operacion_id= 2, tarjeta_id=1});
            await connection.ExecuteAsync(@"INSERT INTO operacion (fecha, tipo_operacion_id, tarjeta_id) values(@fecha, @tipo_operacion_id, @tarjeta_id);", new {fecha = DateTime.Today, tipo_operacion_id= 1, tarjeta_id=1});
        }
    }
}