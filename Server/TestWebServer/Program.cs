using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

// ���� ���
builder.Services.AddControllers(); // MVC ���� �߰�
builder.Services.AddAuthorization(); // ���� �ο� ���� �߰�

var app = builder.Build();

// ��û ���������� ����
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // ���� ����� �� ���� ������ ���
}
else
{
    app.UseExceptionHandler("/Home/Error"); // ���� �߻� �� �ڵ鷯 ���
    app.UseHsts(); // HSTS Ȱ��ȭ
}

app.UseCors("AllowAllOrigins");
app.UseHttpsRedirection(); // HTTPS ���𷺼�
app.UseStaticFiles(); // ���� ���� ����
app.UseRouting(); // ����� ���

app.UseAuthorization(); // ���� �ο� �̵���� ���

app.MapControllers(); // ��Ʈ�ѷ� ����� ����

app.Run(); // ���ø����̼� ����