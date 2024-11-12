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

// 서비스 등록
builder.Services.AddControllers(); // MVC 패턴 추가
builder.Services.AddAuthorization(); // 권한 부여 서비스 추가

var app = builder.Build();

// 요청 파이프라인 구성
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // 개발 모드일 때 예외 페이지 사용
}
else
{
    app.UseExceptionHandler("/Home/Error"); // 오류 발생 시 핸들러 사용
    app.UseHsts(); // HSTS 활성화
}

app.UseCors("AllowAllOrigins");
app.UseHttpsRedirection(); // HTTPS 리디렉션
app.UseStaticFiles(); // 정적 파일 제공
app.UseRouting(); // 라우팅 사용

app.UseAuthorization(); // 권한 부여 미들웨어 사용

app.MapControllers(); // 컨트롤러 라우팅 설정

app.Run(); // 애플리케이션 실행