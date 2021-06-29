using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;

namespace TutumAPI.Helpers
{
    public class Functions
    {
        private static List<Type> allowedTypes = new List<Type>() { typeof(int?),
                                                            typeof(string),
                                                            typeof(decimal),
                                                            typeof(DateTime)};

        /// <summary>
        /// Создает новый экземпляр модели и заполняет его только примитивными типами, без навигационных свойств
        /// </summary>
        public static T getCleanModel<T>(T input)
        {
            T obj = (T)Activator.CreateInstance(typeof(T));
            //Получаем список свойств примитивного типа
            var listOfProperties = obj.GetType().GetProperties().Where(e => e.PropertyType.IsPrimitive || allowedTypes.Contains(e.PropertyType)).ToList();
            //Заполняем значениями входного параметра пустой объект
            listOfProperties.ForEach(e => e.SetValue(obj, e.GetValue(input)));

            return obj;
        }

        /// <summary>
        /// Возвращает список объектов без навигационных свойств
        /// </summary>
        public static List<T> getCleanListOfModels<T>(List<T> input)
        {
            List<T> result = new List<T>();
            input.ForEach(e => result.Add(getCleanModel(e)));
            return result;
        }

        /// <summary>
        /// Возвращает _count элементов начиная со страницы _startingPage
        /// </summary>
        /// <param name="_initialQuery">Изначальный набор</param>
        /// <param name="_startingPage">Начальный индекс выборки</param>
        /// <param name="_pageSize">Количество элементов на странице</param>
        public static IQueryable<T> GetPageRange<T>(IQueryable<T> _initialQuery, int _startingPage, int _pageSize)
        {
            //страница 0, 20 элементов = 0-19 элементы
            //страница 5, 10 элементов = 50-59 элементы
            return _initialQuery.Skip(_startingPage * _pageSize).Take(_pageSize);
        }

        public static bool IsPhoneNumber(string number)
        {
            return Regex.Match(number, @"^((8|\+7|7)[\- ]?)?(\(?\d{3}\)?[\- ]?)?[\d\- ]{7,10}$").Success;
        }

        public static string GetHashFromString(string input)
        {
            var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));

            return Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Конвертирует телефон в единый формат
        /// </summary>
        public static string convertNormalPhoneNumber(string originalNumber)
        {
            if (originalNumber == null)
            {
                return null;
            }
            //Базировал на https://bit.ly/3lEsT2R
            string processedNumber = originalNumber;
            //Сперва удаляем лишние символы
            List<string> junkSymbols = new List<string>()
            {
                "(", ")", "+", "-"
            };
            junkSymbols.ForEach(e => processedNumber = processedNumber.Replace(e, ""));
            //Если в начале нет 7 или 8 - вставить код самому. Пока плевать на интернационализацию
            return "7" + ((processedNumber.StartsWith("7") || processedNumber.StartsWith("8")) ?
                                        processedNumber.Substring(1) : processedNumber);
        }
    }
}