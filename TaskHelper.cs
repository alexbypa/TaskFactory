    public class TaskHelper<T> {
        public T InvokeIntervalList<Y>(List<Y> list, short valueintervalList, Func<Y, T> action, Func<int, int, T> formatResponse, Func<T, bool> ResponseIsvalid) {
            int totSuccess = 0;
            int totFailure = 0;
            do {
                var tasks = new List<Task>();
                int counttasktodetach = list.Count() < valueintervalList ? list.Count() : valueintervalList;
                foreach (var item in list.Take(counttasktodetach)) {
                    T responsetask = default(T);
                    tasks.Add(Task.Run(() => {
                        try {
                            responsetask = action.Invoke(item);
                            if (ResponseIsvalid.Invoke(responsetask))
                                Interlocked.Increment(ref totSuccess);
                            else
                                Interlocked.Increment(ref totFailure);
                        } catch (Exception ex) {
                            Interlocked.Increment(ref totFailure);

                            IConfigurationBuilder builder = new ConfigurationBuilder();
                            builder.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"));
                            var root = builder.Build();
                            var factory = LoggerFactory.Create(b => b.AddCustomLogger(options => root.GetSection("Logging:CustomLogger:Options").Bind(options)));
                            factory.CreateLogger<CustomLogger>().traceError("Cash Ticket Async", ex);
                        }
                    }));
                }
                Task t = Task.WhenAll(tasks);
                try {
                    t.Wait();
                } catch (Exception extask) {

                    IConfigurationBuilder builder = new ConfigurationBuilder();
                    builder.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"));
                    var root = builder.Build();
                    var factory = LoggerFactory.Create(b => b.AddCustomLogger(options => root.GetSection("Logging:CustomLogger:Options").Bind(options)));
                    factory.CreateLogger<CustomLogger>().traceError("Cash Ticket Async", extask);
                }
                //if (t.Status == TaskStatus.RanToCompletion)
                //    req.logger.LogInformation($"{req.action.ToString()} metodo eseguito correttamente con range task asincroni {valueintervalList}");
                //else if (t.Status == TaskStatus.Faulted)
                //    req.logger.traceError(req.action.ToString(), new Exception($"Numero di threads falliti : {totFailure}"));

                list.RemoveRange(0, counttasktodetach);
            } while (list.Count() > 0);
            return formatResponse.Invoke(totSuccess, totFailure);
        }