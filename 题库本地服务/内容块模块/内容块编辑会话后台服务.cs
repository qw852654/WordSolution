using 题库应用.内容块模块;
using 题库基础设施.题库实例;
using 题库核心.内容块模块.契约;

namespace 题库本地服务.内容块模块
{
    public class 内容块编辑会话后台服务 : BackgroundService
    {
        private static readonly TimeSpan 扫描间隔 = TimeSpan.FromSeconds(3);

        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly I内容块编辑会话存储 _内容块编辑会话存储;
        private readonly ILogger<内容块编辑会话后台服务> _logger;

        public 内容块编辑会话后台服务(
            IServiceScopeFactory serviceScopeFactory,
            I内容块编辑会话存储 内容块编辑会话存储,
            ILogger<内容块编辑会话后台服务> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _内容块编辑会话存储 = 内容块编辑会话存储;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    扫描活动会话();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "扫描内容块编辑会话失败。");
                }

                await Task.Delay(扫描间隔, stoppingToken);
            }
        }

        private void 扫描活动会话()
        {
            var 活动会话列表 = _内容块编辑会话存储.获取活动会话();
            foreach (var 分组 in 活动会话列表.GroupBy(会话 => 会话.题库键))
            {
                using var scope = _serviceScopeFactory.CreateScope();
                scope.ServiceProvider.GetRequiredService<当前题库上下文>().设置题库键(分组.Key);
                var 同步用例 = scope.ServiceProvider.GetRequiredService<同步内容块编辑会话用例>();

                foreach (var 会话 in 分组)
                {
                    同步用例.执行(会话.会话ID, false);
                }
            }
        }
    }
}
