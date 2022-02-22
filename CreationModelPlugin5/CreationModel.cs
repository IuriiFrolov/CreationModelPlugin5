using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;

namespace CreationModelPlugin5
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CreationModel : IExternalCommand
    {// Лекция № 4 Создание модели  важная статья https://digitteck.com/dotnet/families-symbols-instances-and-system-families/
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) // добирается до документа, сообщение при неудачи, набор элементов
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;

            Level level1 = GetLevelByName(doc, "Уровень 1"); // используем метод для нахождения уровня
            Level level2 = GetLevelByName(doc, "Уровень 2"); // используем метод для нахождения уровня

            List<Wall> walls = CreateWall(doc, level1, level2, 10000, 5000); // используем метод для создания стен и добавления их в список (документ, низ стены, верх стены, Длина, Ширина)

            return Result.Succeeded;
        }

        // метод для нахождения уровня
        public Level GetLevelByName(Document doc, string nameLevel)
        {
            Level level = new FilteredElementCollector(doc)
                       .OfClass(typeof(Level))
                       .OfType<Level>()
                       .Where(x => x.Name.Equals(nameLevel))
                       .FirstOrDefault();

            return level;
        }

        // Вспомогательный метод для построения опорных точек, передается в CreateWall 
        public List<XYZ> CreatePointsWall(Document doc, double widthInMilimeters, double depthInMilimeters)
        {
            double oWidth = UnitUtils.ConvertToInternalUnits(widthInMilimeters, UnitTypeId.Millimeters);
            double oDepth = UnitUtils.ConvertToInternalUnits(depthInMilimeters, UnitTypeId.Millimeters);
            double dx = oWidth / 2;
            double dy = oDepth / 2;

            List<XYZ> points = new List<XYZ>();
            points.Add(new XYZ(-dx, -dy, 0));
            points.Add(new XYZ(dx, -dy, 0));
            points.Add(new XYZ(dx, dy, 0));
            points.Add(new XYZ(-dx, dy, 0));
            points.Add(new XYZ(-dx, -dy, 0));

            return points;
        }

        // Основной метод для создания 4-х стен и добавления стен в лист.
        public List<Wall> CreateWall(Document doc, Level level1, Level level2, double widthInMilimeters, double depthInMilimeters)
        {
            List<XYZ> points = CreatePointsWall(doc, widthInMilimeters, depthInMilimeters);

            List<Wall> walls = new List<Wall>();
            Transaction transaction = new Transaction(doc, "Построение стен");
            transaction.Start();
            for (int i = 0; i < 4; i++)
            {
                Line line = Line.CreateBound(points[i], points[i + 1]); // Ось стены из 2 точек
                Wall wall = Wall.Create(doc, line, level1.Id, false); // Создаем стену (в активном документе, по линии заданной 2 точками, на 1 уровне низ стены, НЕ НЕСУЩАЯ)
                walls.Add(wall); // Добавляем стену в лист (список) "Стены"
                wall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).Set(level2.Id);  // устанавливаем высоту стены до 2 уровня
            }

            AddDoor(doc, level1, walls[0]);
            AddWindow(doc, level1, walls[1]);
            AddWindow(doc, level1, walls[2]);
            AddWindow(doc, level1, walls[3]);
            //AddRoofLesson4(doc, level2, walls);
            AddRoofHomeWork(doc, level2, walls, widthInMilimeters);

            transaction.Commit();
            return walls;

        }
        //  Д/З замените метод NewFootPrintRoof методом NewExtrusionRoof
        // Метод создания крыши 2х скатной экструзивная
        private void AddRoofHomeWork(Document doc, Level level2, List<Wall> walls, double widthInMilimeters)
        {
            RoofType roofType = new FilteredElementCollector(doc)
                .OfClass(typeof(RoofType))
                .OfType<RoofType>()
                .Where(x => x.Name.Equals("Типовой - 400мм"))
                .Where(x => x.FamilyName.Equals("Базовая крыша"))
                .FirstOrDefault(); //элемент а не коллекция

            double wallWidth = walls[0].Width;
            double dt = wallWidth / 2;

            var heightParametr = walls[0].get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM);
            double heightWall = heightParametr.AsDouble();
            double heightRoof = heightWall + 7;


            List<XYZ> points = new List<XYZ>();
            points.Add(new XYZ(-dt, -dt, 0)); //0
            points.Add(new XYZ(dt, -dt, heightWall)); //1
            points.Add(new XYZ(dt, dt, heightRoof)); //2
            points.Add(new XYZ(-dt, dt, 0));  //3
            points.Add(new XYZ(-dt, -dt, 0)); //0
            points.Add(new XYZ(0, dt, heightWall)); //1

         

            //var heightParametr = Wall.Parameter[BuiltInParameter.WALL_USER_HEIGHT_PARAM];
            // double height = 

            Application application = doc.Application;
            

            CurveArray curveArray = new CurveArray();
            //curveArray.Append(Line.CreateBound(new XYZ(0, 0, 0), new XYZ(0, 20, 20)));
           // curveArray.Append(Line.CreateBound(new XYZ(0, 20, 20), new XYZ(0, 40, 0)));
           
                LocationCurve curve1 = walls[1].Location as LocationCurve;
                XYZ p1 = curve1.Curve.GetEndPoint(0);
                XYZ p3 = curve1.Curve.GetEndPoint(1);
            XYZ p2 = (p1 + p3) / 2;

            curveArray.Append(Line.CreateBound(p1 + points[1], p2 + points[2]));
                curveArray.Append(Line.CreateBound(p2 + points[2], p3 + points[5]));

            double oWidth = UnitUtils.ConvertToInternalUnits(widthInMilimeters, UnitTypeId.Millimeters);
            double dx = (oWidth / 2)+1.64042; // длина стены + свес 




            //  Д/З замените метод NewFootPrintRoof методом NewExtrusionRoof
            ReferencePlane plane = doc.Create.NewReferencePlane(new XYZ(0, 0, 0)
                , new XYZ(0, 0, 20)
                , new XYZ(0, 20, 0)
                , doc.ActiveView);

            ExtrusionRoof extrusionRoof = doc.Create
                .NewExtrusionRoof(curveArray, plane, level2, roofType, -dx, dx);

            // .NewFootPrintRoof(footprint, level2, roofType, out footPrintToModelCurveMapping);

            //foreach (ModelCurve m in footPrintToModelCurveMapping)
            //{
            //    footprintRoof.set_DefinesSlope(m, true);
            //    footprintRoof.set_SlopeAngle(m, 0.5);
            //}


            //FootPrintRoof footprintRoof = doc.Create.NewFootPrintRoof(footprint, level2, roofType, out footPrintToModelCurveMapping);
            //foreach (ModelCurve m in footPrintToModelCurveMapping)
            //{
            //    footprintRoof.set_DefinesSlope(m, true);
            //    footprintRoof.set_SlopeAngle(m, 0.5);
            //}
        }



        // Метод создания крыши 4х скатной по лекции 
        private void AddRoofLesson4(Document doc, Level level2, List<Wall> walls)
        {
            RoofType roofType = new FilteredElementCollector(doc)
                .OfClass(typeof(RoofType))
                .OfType<RoofType>()
                .Where(x => x.Name.Equals("Типовой - 400мм"))
                .Where(x => x.FamilyName.Equals("Базовая крыша"))
                .FirstOrDefault(); //элемент а не коллекция

            double wallWidth = walls[0].Width;
            double dt = wallWidth / 2;
            List<XYZ> points = new List<XYZ>();
            points.Add(new XYZ(-dt, -dt, 0));
            points.Add(new XYZ(dt, -dt, 0));
            points.Add(new XYZ(dt, dt, 0));
            points.Add(new XYZ(-dt, dt, 0));
            points.Add(new XYZ(-dt, -dt, 0));


            Application application = doc.Application;
            CurveArray footprint = application.Create.NewCurveArray(); // отпечаток по которому будет построена крыша
            for (int i = 0; i < 4; i++)
            {
                LocationCurve curve = walls[i].Location as LocationCurve;
                XYZ p1 = curve.Curve.GetEndPoint(0);
                XYZ p2 = curve.Curve.GetEndPoint(1);
                Line line = Line.CreateBound(p1 + points[i], p2 + points[i + 1]);
                footprint.Append(line);
            }
            ModelCurveArray footPrintToModelCurveMapping = new ModelCurveArray();
            FootPrintRoof footprintRoof = doc.Create
                .NewFootPrintRoof(footprint, level2, roofType, out footPrintToModelCurveMapping);

            foreach (ModelCurve m in footPrintToModelCurveMapping)
            {
                footprintRoof.set_DefinesSlope(m, true);
                footprintRoof.set_SlopeAngle(m, 0.5);
            }



            //CurveArray curveArray = new CurveArray();
            //curveArray.Append(Line.CreateBound(new XYZ(0, 0, 0), new XYZ(0, 20, 20)));
            //curveArray.Append(Line.CreateBound(new XYZ(0, 20, 20), new XYZ(0, 40, 0)));
            ////  Д/З замените метод NewFootPrintRoof методом NewExtrusionRoof
            //ReferencePlane plane = doc.Create.NewReferencePlane(new XYZ(0, 0, 0)
            //    , new XYZ(0, 0, 20)
            //    , new XYZ(0, 20, 0)
            //    , doc.ActiveView);
            
            //ExtrusionRoof extrusionRoof = doc.Create
            //    .NewExtrusionRoof(curveArray, plane, level2, roofType, 0, 40);

           // .NewFootPrintRoof(footprint, level2, roofType, out footPrintToModelCurveMapping);

            //foreach (ModelCurve m in footPrintToModelCurveMapping)
            //{
            //    footprintRoof.set_DefinesSlope(m, true);
            //    footprintRoof.set_SlopeAngle(m, 0.5);
            //}


            //FootPrintRoof footprintRoof = doc.Create.NewFootPrintRoof(footprint, level2, roofType, out footPrintToModelCurveMapping);
            //foreach (ModelCurve m in footPrintToModelCurveMapping)
            //{
            //    footprintRoof.set_DefinesSlope(m, true);
            //    footprintRoof.set_SlopeAngle(m, 0.5);
            //}
        }

        private void AddDoor(Document doc, Level level1, Wall wall)
        {
            FamilySymbol doorType = new FilteredElementCollector(doc)
                 .OfClass(typeof(FamilySymbol))
                 .OfCategory(BuiltInCategory.OST_Doors)
                 .OfType<FamilySymbol>()
                 .Where(x => x.Name.Equals("0915 x 2134 мм"))
                 .Where(x => x.FamilyName.Equals("Одиночные-Щитовые"))
                 .FirstOrDefault();

            LocationCurve hostCurve = wall.Location as LocationCurve;
            XYZ point1 = hostCurve.Curve.GetEndPoint(0);
            XYZ point2 = hostCurve.Curve.GetEndPoint(1);
            XYZ point = (point1 + point2) / 2; // (Позиция, Типоразмер, Основа (стена хозяин), Уровень, Не несущая?)

            if (!doorType.IsActive)
                doorType.Activate();

            doc.Create.NewFamilyInstance(point, doorType, wall, level1, StructuralType.NonStructural);
        }

        private void AddWindow(Document doc, Level level1, Wall wall)
        {
            FamilySymbol windowType = new FilteredElementCollector(doc)
                 .OfClass(typeof(FamilySymbol))
                 .OfCategory(BuiltInCategory.OST_Windows)
                 .OfType<FamilySymbol>()
                 .Where(x => x.Name.Equals("0915 x 1830 мм"))
                 .Where(x => x.FamilyName.Equals("Фиксированные"))
                 .FirstOrDefault();

            LocationCurve hostCurve = wall.Location as LocationCurve;
            XYZ point1 = hostCurve.Curve.GetEndPoint(0);
            XYZ point2 = hostCurve.Curve.GetEndPoint(1);

            XYZ point3 = (point1 + point2) / 2; // Вычисляем середину стены

            double windowSillHeight = UnitUtils.ConvertToInternalUnits(900, UnitTypeId.Millimeters); // Указываем высоту подоконника

            XYZ point =  new XYZ(point3.X, point3.Y, windowSillHeight); // собираем точку вставки из "середины стены" и "высоты подоконника"

            if (!windowType.IsActive)
                windowType.Activate();

            doc.Create.NewFamilyInstance(point, windowType, wall, level1, StructuralType.NonStructural); // (Позиция, Типоразмер, Основа (стена хозяин), Уровень, Не несущая)
        }
    }
}



//  var res1= new FilteredElementCollector(doc) /* .OfType - фильтр LINQ (в подсказке написано IEnumerable) !!!,
//                                               * а .OfCategory & .OfCategoryId &  .OfClass  напротив принадлежат Revit (в подсказке написано FilteredElementCollector),
//                                               * необходимо внимательно читать подсказки */
//                .OfClass(typeof(WallType))  /*typeof объектно орентированное представление типа, т.е не человек а его страница в FaceBook */
//                //.Cast<Wall>()  /*метод расширения которые выполняет ПРЕОБРАЗОВАНИЕ каждого элемента в списке к заданному типу <Wall>  не безопасное привидение может выдать исключение*/
//                .OfType<WallType>()   /* метод расширения которые выполняет ФИЛЬТРАЦИЮ на основе заданного типа, безопасное привидение к списку стен, LINQ  работает медленно, ставим в конце*/
//                .ToList();

//var res2 = new FilteredElementCollector(doc)
//    .OfClass(typeof(FamilyInstance))
//    .OfCategory(BuiltInCategory.OST_Doors) // фильтруем все двери
//    .OfType<FamilyInstance>()
//    .Where(x => x.Name.Equals("36 x 84"))   // поиск по имени двери
//    .ToList();

//var res3 = new FilteredElementCollector(doc)
//    //.WhereElementIsElementType() // быстрый фильтр отбирает то что относится к типаразмерам
//    .WhereElementIsNotElementType() // находит экземпляры, т.е конкретно двери, окна ....
//    .ToList();



