using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using static Camelot.Core;

namespace Camelot.Tests
{
    public class TableListTests
    {
        private static Table MakeTable(int? page, int? order)
        {
            return new Table(new List<(float, float)>(), new List<(float, float)>())
            {
                Page = page,
                Order = order
            };
        }

        [Fact]
        public void TableOrder()
        {
            var table_list = new TableList(new List<Table>()
            {
                MakeTable(2, 1),
                MakeTable(1, 1),
                MakeTable(3, 4),
                MakeTable(1, 2)
            });

            table_list.Sort();
            var pageOrder = table_list.Select(t => (t.Page.Value, t.Order.Value)).ToArray();
            Assert.Equal(new[]
            {
                (1, 1),
                (1, 2),
                (2, 1),
                (3, 4)
            }, pageOrder);

            table_list.Reverse();
            var pageOrderReverse = table_list.Select(t => (t.Page.Value, t.Order.Value)).ToArray();
            Assert.Equal(new[]
            {
                (3, 4),
                (2, 1),
                (1, 2),
                (1, 1),
            }, pageOrderReverse);
        }

        [Fact]
        public void TableOrderNull()
        {
            var table_list = new TableList(new List<Table>()
            {
                MakeTable(null, 1),
                MakeTable(1, 1),
                MakeTable(3, 4),
                MakeTable(1, 2)
            });
            Assert.Throws<InvalidOperationException>(() => table_list.Sort());

            var table_list_1 = new TableList(new List<Table>()
            {
                MakeTable(2, 1),
                MakeTable(1, 1),
                MakeTable(3, 4),
                MakeTable(1, null)
            });
            Assert.Throws<InvalidOperationException>(() => table_list_1.Sort());

            // ok with null value
            var table_list_ok = new TableList(new List<Table>()
            {
                MakeTable(2, 1),
                MakeTable(1, 1),
                MakeTable(3, null),
                MakeTable(1, 2)
            });
            table_list_ok.Sort();

            var pageOrder = table_list_ok.Select(t => (t.Page, t.Order)).ToArray();
            Assert.Equal(new (int?, int?)[]
            {
                (1, 1),
                (1, 2),
                (2, 1),
                (3, null)
            }, pageOrder);

            table_list_ok.Reverse();
            var pageOrderReverse = table_list_ok.Select(t => (t.Page, t.Order)).ToArray();
            Assert.Equal(new (int?, int?)[]
            {
                (3, null),
                (2, 1),
                (1, 2),
                (1, 1),
            }, pageOrderReverse);
        }
    }
}
