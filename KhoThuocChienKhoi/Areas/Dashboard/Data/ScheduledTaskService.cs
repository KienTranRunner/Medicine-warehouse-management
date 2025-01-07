using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KhoThuocChienKhoi.Models;
using Microsoft.EntityFrameworkCore;

namespace KhoThuocChienKhoi.Areas.Dashboard.Data
{
    public class ScheduledTaskService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public ScheduledTaskService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;

                if (now.Day == 29 && now.Hour == 23 && now.Minute == 59 && now.Second == 59)
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<KhoThuocChienKhoiContext>();

                        // Gọi stored procedure
                        await context.Database.ExecuteSqlRawAsync("EXEC usp_UpdateTonDauForNextMonth");
                    }

                    // Chờ 24 giờ để không lặp lại trong cùng ngày
                    await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
                }
                else
                {
                    // Kiểm tra lại sau 1 phút
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }


            }
        }
    }
}