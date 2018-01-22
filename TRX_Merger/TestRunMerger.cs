using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRX_Merger.TrxModel;
using TRX_Merger.Utilities;

namespace TRX_Merger
{
    public static class TestRunMerger
    {

        public static TestRun MergeTRXsAndSave(List<string> trxFiles, string outputFile)
        {
            Console.WriteLine("Deserializing trx files:");
            List<TestRun> runs = new List<TestRun>();
            foreach (var trx in trxFiles)
            {
                Console.WriteLine(trx);
                runs.Add(TRXSerializationUtils.DeserializeTRX(trx));
            }

            Console.WriteLine("Combining deserialized trx files...");
            var combinedTestRun = MergeTestRuns(runs);

            Console.WriteLine("Saving result...");
            var savedFile = TRXSerializationUtils.SerializeAndSaveTestRun(combinedTestRun, outputFile);

            Console.WriteLine("Operation completed:");
            Console.WriteLine("\tCombined trx files: " + trxFiles.Count);
            Console.WriteLine("\tResult trx file: " + savedFile);

            return combinedTestRun;
        }

        private static TestRun MergeTestRuns(List<TestRun> testRuns)
        {
            int index = 0;
            string name = testRuns[0].Name;
            string runUser = testRuns[0].RunUser;

            string startString = "";
            DateTime startDate = DateTime.MaxValue;

            string endString = "";
            DateTime endDate = DateTime.MinValue;




            List<UnitTestResult> allResults = new List<UnitTestResult>();
            List<UnitTest> allTestDefinitions = new List<UnitTest>();
            List<TestEntry> allTestEntries = new List<TestEntry>();
            List<TestList> allTestLists = new List<TestList>();


            var resultSummary = new ResultSummary
            {
                Counters = new Counters(),
                RunInfos = new List<RunInfo>(),
            };
//            bool resultSummaryPassed = true;

            foreach (var tr in testRuns)
            {
                if (index!=0)
                {
                    foreach (var untTestRlt in tr.Results)
                    {
                        allResults.RemoveAll(utr => utr.TestId == untTestRlt.TestId && Convert.ToDateTime(untTestRlt.StartTime)>Convert.ToDateTime(utr.StartTime));
                        allResults.Add(untTestRlt);
                    }

                    foreach (var te in tr.TestEntries)
                    {
                        allTestEntries.RemoveAll(testEnt => testEnt.TestId == te.TestId);
                        allTestEntries.Add(te);
                    }
                    foreach (var td in tr.TestDefinitions)
                    {
                        allTestDefinitions.RemoveAll(testDef => testDef.Id == td.Id);
                        allTestDefinitions.Add(td);
                    }
                }
                else
                {
                    allResults = allResults.Concat(tr.Results).ToList();                  
                    allTestEntries = allTestEntries.Concat(tr.TestEntries).ToList();
                    allTestDefinitions = allTestDefinitions.Concat(tr.TestDefinitions).ToList();
                    resultSummary.Counters.Total = tr.ResultSummary.Counters.Total;
                }             
                allTestLists = allTestLists.Concat(tr.TestLists).ToList();


                DateTime currStart = DateTime.Parse(tr.Times.Start);
                if (currStart < startDate)
                {
                    startDate = currStart;
                    startString = tr.Times.Start;
                }

                DateTime currEnd = DateTime.Parse(tr.Times.Finish);
                if (currEnd > endDate)
                {
                    endDate = currEnd;
                    endString = tr.Times.Finish;
                }
                    resultSummary.RunInfos = resultSummary.RunInfos.Concat(tr.ResultSummary.RunInfos).ToList();
                    //resultSummary.Counters.Aborted = tr.ResultSummary.Counters.Aborted;
                    //resultSummary.Counters.Completed += tr.ResultSummary.Counters.Completed;
                    //resultSummary.Counters.Disconnected += tr.ResultSummary.Counters.Disconnected;
                    //resultSummary.Counters.Еxecuted += tr.ResultSummary.Counters.Еxecuted;
                    //resultSummary.Counters.Failed += tr.ResultSummary.Counters.Failed;
                    //resultSummary.Counters.Inconclusive += tr.ResultSummary.Counters.Inconclusive;
                    //resultSummary.Counters.InProgress += tr.ResultSummary.Counters.InProgress;
                    //resultSummary.Counters.NotExecuted += tr.ResultSummary.Counters.NotExecuted;
                    //resultSummary.Counters.NotRunnable += tr.ResultSummary.Counters.NotRunnable;
                    //resultSummary.Counters.Passed += tr.ResultSummary.Counters.Passed;
                    //resultSummary.Counters.PassedButRunAborted += tr.ResultSummary.Counters.PassedButRunAborted;
                    //resultSummary.Counters.Pending += tr.ResultSummary.Counters.Pending;
                    //resultSummary.Counters.Timeout += tr.ResultSummary.Counters.Timeout;
                    //resultSummary.Counters.Total += tr.ResultSummary.Counters.Total;
                    //resultSummary.Counters.Warning += tr.ResultSummary.Counters.Warning;


                index++;
            }

            resultSummary.Counters.Aborted = allResults.FindAll(res => res.Outcome == "Aborted").Count;
            resultSummary.Counters.Completed = allResults.FindAll(res => res.Outcome == "Completed").Count;
            resultSummary.Counters.Disconnected = allResults.FindAll(res => res.Outcome == "Disconnected").Count;
            resultSummary.Counters.Failed = allResults.FindAll(res => res.Outcome == "Failed").Count;
            resultSummary.Counters.Inconclusive = allResults.FindAll(res => res.Outcome == "Inconclusive").Count;
            resultSummary.Counters.InProgress = allResults.FindAll(res => res.Outcome == "InProgress").Count;
            resultSummary.Counters.NotExecuted = allResults.FindAll(res => res.Outcome == "NotExecuted").Count;
            resultSummary.Counters.NotRunnable = allResults.FindAll(res => res.Outcome == "NotRunnable").Count;
            resultSummary.Counters.Passed = allResults.FindAll(res => res.Outcome == "Passed").Count;
            resultSummary.Counters.PassedButRunAborted = allResults.FindAll(res => res.Outcome == "PassedButRunAborted").Count;
            resultSummary.Counters.Pending = allResults.FindAll(res => res.Outcome == "Pending").Count;
            resultSummary.Counters.Timeout = allResults.FindAll(res => res.Outcome == "Timeout").Count;           
            resultSummary.Counters.Warning = allResults.FindAll(res => res.Outcome == "Warning").Count;
            resultSummary.Counters.Еxecuted = allResults.Count;

            resultSummary.Outcome = (resultSummary.Counters.Total - resultSummary.Counters.Passed == 0) ? "Passed" : "Failed";


            return new TestRun
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                RunUser = runUser,
                Times = new Times
                {
                    Start = startString,
                    Queuing = startString,
                    Creation = startString,
                    Finish = endString,
                },
                Results = allResults,
                TestDefinitions = allTestDefinitions,
                TestEntries = allTestEntries,
                TestLists = allTestLists,
                ResultSummary = resultSummary,
            };
        } 
 
    }
}
