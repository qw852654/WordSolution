using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace TagRunner.索引
{
    /// <summary>
    /// 索引工作者：内存队列的消费者，按任务类型执行索引或删除操作。
    /// 简单实现：使用 BlockingCollection 支撑生产/消费，运行单后台线程处理。
    /// </summary>
    public class 索引工作者
    {
        private readonly I索引服务 _索引服务;
        private readonly BlockingCollection<索引任务> _队列 = new BlockingCollection<索引任务>(new ConcurrentQueue<索引任务>());
        private CancellationTokenSource _cts;
        private Task _工作任务;

        public 索引工作者(I索引服务 索引服务)
        {
            _索引服务 = 索引服务 ?? throw new ArgumentNullException(nameof(索引服务));
        }

        public void 启动()
        {
            _cts = new CancellationTokenSource();
            _索引服务.启动();
            _工作任务 = Task.Factory.StartNew(() => 运行Loop(_cts.Token), TaskCreationOptions.LongRunning);
        }

        public void 停止()
        {
            _队列.CompleteAdding();
            _cts.Cancel();
            try { _工作任务?.Wait(); } catch { }
            _索引服务.停止();
        }

        public void Enqueue索引(int 题目Id, string html路径)
        {
            _队列.Add(new 索引任务 { 题目Id = 题目Id, Html路径 = html路径, 类型 = 索引任务类型.索引 });
        }

        public void Enqueue删除(int 题目Id)
        {
            _队列.Add(new 索引任务 { 题目Id = 题目Id, 类型 = 索引任务类型.删除 });
        }

        private void 运行Loop(CancellationToken token)
        {
            try
            {
                foreach (var task in _队列.GetConsumingEnumerable(token))
                {
                    try
                    {
                        if (task.类型 == 索引任务类型.索引)
                        {
                            _索引服务.索引文档(task.题目Id, task.Html路径);
                        }
                        else if (task.类型 == 索引任务类型.删除)
                        {
                            _索引服务.删除文档(task.题目Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        // TODO: 记录错误并根据需要重试或放入失败队列
                    }

                    if (token.IsCancellationRequested) break;
                }
            }
            catch (OperationCanceledException) { }
        }
    }

    internal class 索引任务
    {
        public int 题目Id { get; set; }
        public string Html路径 { get; set; }
        public 索引任务类型 类型 { get; set; }
    }

    internal enum 索引任务类型
    {
        索引 = 0,
        删除 = 1
    }
}
