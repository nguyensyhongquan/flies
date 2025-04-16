    using System;
    using Microsoft.EntityFrameworkCore.Migrations;

    #nullable disable

    namespace FliesProject.Migrations
    {
        /// <inheritdoc />
        public partial class UpdateCourseSchema : Migration
        {
            /// <inheritdoc />
            protected override void Up(MigrationBuilder migrationBuilder)
            {
                migrationBuilder.DropIndex(
                    name: "UQ__Users__85FB4E38CD92FC1E",
                    table: "Users");

                migrationBuilder.AlterColumn<int>(
                    name: "timelimit",
                    table: "Courses",
                    type: "int",
                    nullable: false,
                 
                    oldClrType: typeof(int),
                    oldType: "int",
                    oldNullable: true);

                migrationBuilder.AlterColumn<decimal>(
                    name: "price",
                    table: "Courses",
                    type: "decimal(18,2)",
                    nullable: false,
               
                    oldClrType: typeof(decimal),
                    oldType: "decimal(18,2)",
                    oldNullable: true);

                migrationBuilder.AlterColumn<string>(
                    name: "description",
                    table: "Courses",
                    type: "text",
                    nullable: false,
                    defaultValue: "",
                    oldClrType: typeof(string),
                    oldType: "text",
                    oldNullable: true);

                migrationBuilder.AlterColumn<DateTime>(
                    name: "created_at",
                    table: "Courses",
                    type: "datetime",
                    nullable: false,
                    defaultValueSql: "(getdate())",
                    oldClrType: typeof(DateTime),
                    oldType: "datetime",
                    oldNullable: true,
                    oldDefaultValueSql: "(getdate())");
            }

            /// <inheritdoc />
            protected override void Down(MigrationBuilder migrationBuilder)
            {
                migrationBuilder.AlterColumn<int>(
                    name: "timelimit",
                    table: "Courses",
                    type: "int",
                    nullable: true,
                    oldClrType: typeof(int),
                    oldType: "int");

                migrationBuilder.AlterColumn<decimal>(
                    name: "price",
                    table: "Courses",
                    type: "decimal(18,2)",
                    nullable: true,
                    oldClrType: typeof(decimal),
                    oldType: "decimal(18,2)");

                migrationBuilder.AlterColumn<string>(
                    name: "description",
                    table: "Courses",
                    type: "text",
                    nullable: true,
                    oldClrType: typeof(string),
                    oldType: "text");

                migrationBuilder.AlterColumn<DateTime>(
                    name: "created_at",
                    table: "Courses",
                    type: "datetime",
                    nullable: true,
                    defaultValueSql: "(getdate())",
                    oldClrType: typeof(DateTime),
                    oldType: "datetime",
                    oldDefaultValueSql: "(getdate())");

                migrationBuilder.CreateIndex(
                    name: "UQ__Users__85FB4E38CD92FC1E",
                    table: "Users",
                    column: "PhoneNumber",
                    unique: true,
                    filter: "[PhoneNumber] IS NOT NULL");
            }
        }
    }
