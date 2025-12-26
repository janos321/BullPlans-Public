using Org.BouncyCastle.Asn1.Pkcs;

namespace Workout.Properties.class_interfaces.Other
{
    public class Offer
    {
        public QuestionData mainData;
        public List<QuestionsSide> questionsSide;
        public bool aktiveOffer = true;

        public Offer()
        {
            questionsSide = new List<QuestionsSide>();
        }

        public Offer(QuestionData mainData, List<QuestionsSide> questionsSide)
        {
            this.mainData = mainData;
            this.questionsSide = questionsSide;
        }

        public void Print()
        {
            Console.WriteLine("===== OFFER =====");
            Console.WriteLine($"Aktív: {aktiveOffer}");
            Console.WriteLine();

            if (mainData != null)
            {
                Console.WriteLine(">>> MAIN DATA <<<");
                Console.WriteLine($"Name: {mainData.name}");
                Console.WriteLine($"UserName: {mainData.userName}");
                Console.WriteLine($"Price: {mainData.price}");
                Console.WriteLine();

                if (mainData.tags != null && mainData.tags.Count > 0)
                {
                    Console.WriteLine("Tags:");
                    foreach (var tag in mainData.tags)
                    {
                        Console.WriteLine($"  - Tag: {tag.name}");
                        if (tag.questions != null && tag.questions.Count > 0)
                        {
                            Console.WriteLine("    Questions:");
                            foreach (var q in tag.questions)
                                Console.WriteLine($"      • {q}");
                        }
                        if (tag.exampleAnswer != null && tag.exampleAnswer.Count > 0)
                        {
                            Console.WriteLine("    Example answers:");
                            foreach (var e in tag.exampleAnswer)
                                Console.WriteLine($"      • {e}");
                        }
                    }
                    Console.WriteLine();
                }

                if (mainData.answers != null && mainData.answers.Count > 0)
                {
                    Console.WriteLine("Answers:");
                    foreach (var kv in mainData.answers)
                        Console.WriteLine($"  {kv.Key}: {kv.Value}");
                    Console.WriteLine();
                }
            }

            if (questionsSide != null && questionsSide.Count > 0)
            {
                Console.WriteLine(">>> QUESTIONS SIDE <<<");
                foreach (var q in questionsSide)
                {
                    Console.WriteLine($"Title: {q.mainTittle}");
                    Console.WriteLine($"Main Question: {q.mainQuestion}");
                    Console.WriteLine($"Side Type: {q.sideType}");

                    if (q.questionList != null && q.questionList.Count > 0)
                    {
                        Console.WriteLine("Questions:");
                        foreach (var item in q.questionList)
                            Console.WriteLine($"  • {item}");
                    }

                    Console.WriteLine();
                }
            }

            Console.WriteLine("=================\n");
        }
    }

    public class QuestionsSide
    {
        public string mainTittle { get; set; }
        public string mainQuestion { get; set; }
        public string sideType { get; set; }
        public List<string> questionList { get; set; }

        public QuestionsSide() { questionList = new List<string>(); }

        public QuestionsSide(string mainTittle, string mainQuestion, string sideType, List<string> questionList)
        {
            this.mainTittle = mainTittle;
            this.mainQuestion = mainQuestion;
            this.sideType = sideType;
            this.questionList = questionList;
        }
    }

    public class QuestionData
    {
        public string name { get; set; }
        public string userName { get; set; }
        public string price { get; set; }
        public List<Tag> tags { get; set; }
        public Dictionary<string, string> answers { get; set; }

        public QuestionData(string name, string userName, string price, List<Tag> tags, Dictionary<string, string> answers)
        {
            this.name = name;
            this.userName = userName;
            this.price = price;
            this.tags = tags;
            this.answers = answers;
        }
    }

    public class Tag
    {
        public string name { get; set; }
        public List<string> questions { get; set; }
        public List<string> exampleAnswer { get; set; }

        public Tag(string name, List<string> questions, List<string> exampleAnswer)
        {
            this.name = name;
            this.questions = questions;
            this.exampleAnswer = exampleAnswer;
        }

    }

}
