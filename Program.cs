using System;
using System.Collections;

namespace TrekantCalculator
{

    public class InvalidTriangleException : Exception
    {
        public InvalidTriangleException(string message)
            : base(message)
        {
        }
    }

    public class Triangle
    {
        public double? A = null;
        public double? a = null;
        public double? B = null;
        public double? b = null;
        public double? C = null;
        public double? c = null;

        public Triangle() { }
        public Triangle(double? A, double? a, double? B, double? b, double? C, double? c)
        {
            this.A = A;
            this.a = a;
            this.B = B;
            this.b = b;
            this.C = C;
            this.c = c;
        }
        public Triangle(Dictionary<string, double?> valuePairs)
        {
            try
            {
                this.A = valuePairs["A"];
                this.a = valuePairs["a"];
                this.B = valuePairs["B"];
                this.b = valuePairs["b"];
                this.C = valuePairs["C"];
                this.c = valuePairs["c"];
            }
            catch(KeyNotFoundException)
            {
                throw new InvalidTriangleException("triangle dictionary data is not valid");
            }
        }
        public Triangle(Dictionary<string, SingleQuestion> valuePairs)
        {
            Dictionary<string, double?> dict = new();
            foreach(KeyValuePair<string, SingleQuestion> pair in valuePairs)
            {
                dict.Add(pair.Key, pair.Value.value);
            }
            Triangle tri = new Triangle(dict);

            this.A = tri.A;
            this.a = tri.a;
            this.B = tri.B;
            this.b = tri.b;
            this.C = tri.C;
            this.c = tri.c;
        }

        public void rotateClockWise()
        {
            double? tempValue1 = B;
            double? tempValue2 = b;

            B = A;
            b = a;

            A = C;
            a = c;

            C = tempValue1;
            c = tempValue2;
        }

        public void rotateCounterClockWise()
        {
            double? tempvalue1 = A;
            double? tempvalue2 = a;

            A = B;
            a = b;

            B = C;
            b = c;

            C = tempvalue1;
            c = tempvalue2;
        }
    }
    public enum valueType
    {
        angle,
        distance
    }
    public struct IntStringType
    {
        public Int32 value;
        valueType type;
        public IntStringType (string value, valueType type)
        {
            if(value == null) { throw new InvalidDataException(value+"is not a valid string"); }
            this.value = Int32.Parse(value);
            this.type = type;
        }
        public IntStringType (valueType type)
        {
            this.type = type;
        }
        public IntStringType (int value)
        {
            this.value = value;
        }
        public static explicit operator IntStringType(string stringValue)
        {
            if(stringValue == null) { throw new InvalidDataException(stringValue+"is not a valid string"); }
            return new IntStringType { value = Int32.Parse(stringValue) };
        }
    }
    public struct SingleQuestion
    {
        public string name;
        public double? value;
        public valueType type;

        public SingleQuestion(string name, double value, valueType type)
        {
            this.name = name;
            this.value = value;
            this.type = type;
        }
    }
    public static class Helper
    {
        public static T[] ConcatArray<T>(this T[] x, T[] y)
        {
            if (x == null) throw new ArgumentNullException("x");
            if (y == null) throw new ArgumentNullException("y");
            int oldLen = x.Length;
            Array.Resize<T>(ref x, x.Length + y.Length);
            Array.Copy(y, 0, x, oldLen, y.Length);
            return x;
        }
        public static string toLowerCase(string value)
        {
            char[] array = value.ToCharArray();
            for(int i = 0; i < array.Length; i++)
            {
                array[i] = Char.ToLower(array[i]);
            }

            return new string(array);
        }
        public static bool CheckAnswer(string? answer, string[]? allowedAnswers = null, bool allowAllCaseCharacters = true)
        {
            if(answer == null) { return false; }

            if(allowedAnswers != null)
            {
                if(allowAllCaseCharacters)
                {
                    answer = toLowerCase(answer);
                    for(int i = 0; i < allowedAnswers.Length; i++)
                    {
                        allowedAnswers[i] = toLowerCase(allowedAnswers[i]);
                    }
                }

                if(!allowedAnswers.ToList().Contains(answer)) { return false; }
            }

            return true;
        }
        public static string askQuestion(string question, string[]? acceptableStrings = null, bool allowAllCaseCharacters = true)
        {
            string? answer = null;
            while(answer == null)
            {
                Console.Write(question);
                answer = Console.ReadLine();
                if(!CheckAnswer(answer, acceptableStrings, allowAllCaseCharacters)) { Console.WriteLine("incorrect answer: "+answer); answer = null; }
            }

            return answer;
        }
        public static bool AskTrueFalseQuestion(string question, string[] acceptStrings, string[] denyStrings, bool allowAllCaseCharacters = true)
        {
            string answer = askQuestion(question, ConcatArray<string>(acceptStrings, denyStrings));
            if(denyStrings.ToList().Contains(answer)) { return false; }
            return true;
        }
    }
    public static class TriangleCalculator
    {
        const double RAD_TO_DEG = 180.0 / Math.PI;
        const double DEG_TO_RAD = Math.PI / 180.0;
        static Triangle trySolvingByTotalAngle(Triangle triangle)
        {
            int index = getUnsolvedSideIndex(triangle);
            
            if(index == 1) { triangle.rotateCounterClockWise(); }
            else if(index == 2) { triangle.rotateClockWise(); }

            try
            {
                if(triangle.A == null || triangle.B == null) { return triangle; }
                double A = (double)triangle.A;
                double B = (double)triangle.B;
                triangle.C = 180 - A - B;
            }
            finally
            {
                if(index == 1) { triangle.rotateClockWise(); }
                else if(index == 2) { triangle.rotateCounterClockWise(); }
            }

            return triangle;
        }
        static Triangle trySolveAnglesWithSinusRule(Triangle triangle)
        {
            int index = 0;
            bool foundCalculation = getOpositeValues(triangle, out index);

            if(!foundCalculation) { return triangle; }

            if(index == 2) { triangle.rotateCounterClockWise(); }
            else if(index == 3) { triangle.rotateClockWise(); }
            
            try
            {
                if(triangle.A == null || triangle.a == null) { Console.WriteLine("something went wrong when calculating you triangle angles, please try again and fix any potential errors"); return triangle; }

                if(triangle.b != null)
                {
                    double A = (double)triangle.A;
                    double b = (double)triangle.b;
                    double a = (double)triangle.a;
                    double tempInRadians = Math.Asin((b * Math.Sin(A * DEG_TO_RAD)) / a); // Input angle converted to radians.
                    triangle.B = tempInRadians * RAD_TO_DEG; // Result converted back to degrees.
                }
                if(triangle.c != null)
                {
                    double A = (double)triangle.A;
                    double c = (double)triangle.c;
                    double a = (double)triangle.a;
                    double tempInRadians = Math.Asin((c * Math.Sin(A * DEG_TO_RAD)) / a); // Input angle converted to radians.
                    triangle.C = tempInRadians * RAD_TO_DEG; // Result converted back to degrees.
                }
            }
            finally
            {
                if(index == 2) { triangle.rotateClockWise(); }
                else if(index == 3) { triangle.rotateCounterClockWise(); }
            }

            return triangle;
        }
        static Triangle trySolveLengthsWithPythagoras(Triangle triangle)
        {
            int index = IsRightAngleTriangle(triangle);

            //rotate the 90 angle to be the A angle
            if(index == 2) { triangle.rotateCounterClockWise(); }
            else if(index == 3) { triangle.rotateClockWise(); }

            try
            {
                // if a isn't found but b and c is
                if(triangle.a == null && triangle.b != null && triangle.c != null)
                {
                    double b = (double)triangle.b;
                    double c = (double)triangle.c;
                    triangle.a = Math.Sqrt(Math.Pow(b, 2) + Math.Pow(c, 2));
                }

                // if c isn't found but a and b are
                if(triangle.a != null && triangle.b != null && triangle.c == null)
                {
                    double a = (double)triangle.a;
                    double b = (double)triangle.b;
                    triangle.c = Math.Sqrt(Math.Pow(a, 2) - Math.Pow(b, 2));
                }

                // if b isn't found but a and c are
                if(triangle.a != null && triangle.b == null && triangle.c != null)
                {
                    double a = (double)triangle.a;
                    double c = (double)triangle.c;
                    triangle.b = Math.Sqrt(Math.Pow(a, 2) - Math.Pow(c, 2));
                }
            }
            finally
            {
                //rotates it back to its prior state
                if(index == 2) { triangle.rotateClockWise(); }
                else if(index == 3) { triangle.rotateCounterClockWise(); }
            }



            return triangle;
        }
        static Triangle TrySolveLengthsWithCosineRule(Triangle triangle)
        {
            if(!CanPerformCosineRuleSolve(triangle)) { return triangle; }

            int index = getUnsolvedSideIndex(triangle);
            Console.WriteLine(index);
            if(index == 0) { return triangle; }

            if(index == 1) { triangle.rotateCounterClockWise(); }
            else if(index == 2) { triangle.rotateClockWise(); }
            
            Console.WriteLine("c is: {0}", triangle.c);
            Console.WriteLine("test: {0}, {1}, {2}", triangle.C, triangle.a, triangle.b);
            try
            {
                // should never be possible doe to previus checks, however is done to remove compiler warning
                if(triangle.C == null || triangle.a == null || triangle.b == null) { return triangle; }
                double C = (double)triangle.C;
                double a = (double)triangle.a;
                double b = (double)triangle.b;

                double Temp = Math.Pow(a, 2) + Math.Pow(b, 2);
                Temp = Temp - (2 * a * b * Math.Cos(C * DEG_TO_RAD));
                Console.WriteLine("midcalc c value is: {0}", Math.Sqrt(Temp));
                triangle.c = Math.Sqrt(Temp);
            }
            finally
            {
                Console.WriteLine("now c is: {0}", triangle.c);

                if(index == 1) { triangle.rotateClockWise(); }
                else if(index == 2) { triangle.rotateCounterClockWise(); }
            }

            return triangle;
        }
        static bool CanPerformCosineRuleSolve(Triangle triangle)
        {
            int solvedSides = 0;
            if(triangle.a != null) { solvedSides++; }
            if(triangle.b != null) { solvedSides++; }
            if(triangle.c != null) { solvedSides++; }

            if(solvedSides >= 2) { return true; }

            return false;
        }

        public static Triangle solveTriangle(Triangle triangle)
        {
            if(isTriangleSolved(triangle)) { return triangle; }

            //if(!getOpositeValues(triangle, out index)) { Console.WriteLine("triangle can't be solved with the sinus rule"); return triangle; }

            bool isRight = IsRightAngleTriangle(triangle) == 0 ? false : true;

            bool hasChanged = false;
            int hasChangedCounter = 0;
            while(!isTriangleSolved(triangle))
            {
                hasChanged = false;
                Triangle oldTriangle = triangle;

                triangle = trySolvingByTotalAngle(triangle);

                triangle = trySolveAnglesWithSinusRule(triangle);

                if(isRight)
                {
                    triangle = trySolveLengthsWithPythagoras(triangle);
                }
                else if(CanPerformCosineRuleSolve(triangle))
                {
                    triangle = TrySolveLengthsWithCosineRule(triangle);
                }

                if (triangle.A != oldTriangle.A || triangle.a != oldTriangle.a || 
                    triangle.B != oldTriangle.B || triangle.b != oldTriangle.b || 
                    triangle.C != oldTriangle.C || triangle.c != oldTriangle.c) 
                {
                    hasChanged = true;
                }

                if(!hasChanged) { hasChangedCounter++; }
                else { hasChangedCounter = 0; }

                if(hasChangedCounter >= 3) { Console.WriteLine("unable to solve triangle any more"); break; }
            }

            return triangle;
        }
        public static bool canPartiallySolveTriangle(Triangle triangle)
        {
            if (getOpositeValues(triangle, out _)) { return true; }

            if (CanPerformCosineRuleSolve(triangle)) { return true; }

            return false;
        }
        static bool getOpositeValues(Triangle triangle, out int valueIndex)
        {
            if(triangle.A != null && triangle.a != null) { valueIndex = 1; return true; }
            if(triangle.B != null && triangle.b != null) { valueIndex = 2; return true; }
            if(triangle.C != null && triangle.c != null) { valueIndex = 3; return true; }

            valueIndex = 0;
            return false;
        }
        static bool isTriangleSolved(Triangle triangle)
        {
            if(triangle.A == null) { return false; }
            if(triangle.a == null) { return false; }
            if(triangle.B == null) { return false; }
            if(triangle.b == null) { return false; }
            if(triangle.C == null) { return false; }
            if(triangle.c == null) { return false; }

            return true;
        }
        static int IsRightAngleTriangle(Triangle triangle)
        {
            if(triangle.A <= 90.001 && triangle.A >= 89.999) {return 1;}
            if(triangle.B <= 90.001 && triangle.B >= 89.999) {return 2;}
            if(triangle.C <= 90.001 && triangle.C >= 89.999) {return 3;}

            return 0;
        }
        static int getUnsolvedSideIndex(Triangle triangle)
        {
            int amount = 0;
            if(triangle.a == null) { amount += 1; }
            if(triangle.b == null) { amount += 2; }
            if(triangle.c == null) { amount += 4; }

            switch (amount)
            {
                case 1:
                    return 1;
                case 2:
                    return 2;
                case 4:
                    return 3;
                default:
                    return 0;
            }
        }
    }

    public class CalculatorApp
    {
        static Dictionary<string, SingleQuestion>? questions;
        static bool shouldRun = true;
        static void Main()
        {
            while(shouldRun)
            {
                questions = new Dictionary<string, SingleQuestion>();
                questions.Add("A", new SingleQuestion("A", 0, valueType.angle));
                questions.Add("a", new SingleQuestion("a", 0, valueType.distance));
                questions.Add("B", new SingleQuestion("B", 0, valueType.angle));
                questions.Add("b", new SingleQuestion("b", 0, valueType.distance));
                questions.Add("C", new SingleQuestion("C", 0, valueType.angle));
                questions.Add("c", new SingleQuestion("c", 0, valueType.distance));

                getTriangleValues();

                if(TriangleCalculator.canPartiallySolveTriangle(new Triangle(questions)))
                {
                    Task<Triangle> calculationTask = Task<Triangle>.Factory.StartNew(new Func<Triangle>(
                        () => {
                            return TriangleCalculator.solveTriangle(new Triangle(questions));
                        })
                    );

                    calculationTask.Wait();
                    
                    Triangle result = calculationTask.Result;

                    Console.WriteLine("result:");
                    PrintTriangle(result);

                    string[] correctStrings = ["yes", "y"];
                    string[] wrongStrings = ["no", "n"];
                    shouldRun = Helper.AskTrueFalseQuestion("continue? \n (Y/n): ", correctStrings, wrongStrings);

                    continue;
                }
            }
        }

        static void getTriangleValues()
        {
            bool isCorrect = false;
            while (!isCorrect)
            {
                if(questions == null) { return; }

                getSingleTriangleValues();

                PrintTriangle(new Triangle(questions));
                
                string[] correctStrings = ["yes", "y"];
                string[] wrongStrings = ["no", "n"];
                Console.WriteLine("is this infomation correct");
                isCorrect = Helper.AskTrueFalseQuestion("(Y/n): ", correctStrings, wrongStrings);
            }
        }

        static void PrintTriangle(Triangle triangle)
        {
            Console.WriteLine("value A is {0}", triangle.A);
            Console.WriteLine("value a is {0}", triangle.a);
            Console.WriteLine("value B is {0}", triangle.B);
            Console.WriteLine("value b is {0}", triangle.b);
            Console.WriteLine("value C is {0}", triangle.C);
            Console.WriteLine("value c is {0}", triangle.c);
        }

        static void getSingleTriangleValues()
        {
            if(questions == null) { return; }

            foreach(KeyValuePair<string, SingleQuestion> pair in questions)
            {
                string? value = null;
                SingleQuestion qst = questions[pair.Key];
                while (value == null)
                {
                    Console.Write("triangle value {0} is: ", pair.Value.name);
                    value = Console.ReadLine();
                    if(value == null || value == "") { Console.WriteLine("value marked as unknown"); qst.value = null; break; }
                    
                    try
                    {
                        qst.value = double.Parse(value);
                    }
                    catch(FormatException)
                    {
                        Console.WriteLine(value+" is not a valid number, please try again");
                        continue;
                    }
                }
                questions[pair.Key] = qst;
            }
        }
    }
}
