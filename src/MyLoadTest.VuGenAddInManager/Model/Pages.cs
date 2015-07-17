// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace MyLoadTest.VuGenAddInManager.Model
{
    public sealed class Pages : ObservableCollection<Page>
    {
        public const int DefaultPageSize = 10;
        public const int DefaultMaximumSelectablePages = 5;

        private int _pageSize = DefaultPageSize;
        private int _selectedPageNumber = 1;
        private int _maximumSelectablePages = DefaultMaximumSelectablePages;
        private int _totalItems;
        private int _itemsOnSelectedPage;

        public int TotalItems
        {
            get
            {
                return _totalItems;
            }

            set
            {
                if (_totalItems != value)
                {
                    _totalItems = value;
                    UpdatePages();
                }
            }
        }

        public int SelectedPageNumber
        {
            get
            {
                return _selectedPageNumber;
            }

            set
            {
                if (_selectedPageNumber != value)
                {
                    _selectedPageNumber = value;
                    UpdatePages();
                }
            }
        }

        public int MaximumSelectablePages
        {
            get
            {
                return _maximumSelectablePages;
            }

            set
            {
                if (_maximumSelectablePages != value)
                {
                    _maximumSelectablePages = value;
                    UpdatePages();
                }
            }
        }

        public int ItemsBeforeFirstPage
        {
            get
            {
                return (_selectedPageNumber - 1) * _pageSize;
            }
        }

        public bool IsPaged
        {
            get
            {
                return _totalItems > _pageSize;
            }
        }

        public bool HasPreviousPage
        {
            get
            {
                return IsPaged && !IsFirstPageSelected;
            }
        }

        public bool HasNextPage
        {
            get
            {
                return IsPaged && !IsLastPageSelected;
            }
        }

        public int TotalPages
        {
            get
            {
                return (_totalItems + _pageSize - 1) / _pageSize;
            }
        }

        public int PageSize
        {
            get
            {
                return _pageSize;
            }

            set
            {
                if (_pageSize != value)
                {
                    _pageSize = value;
                    UpdatePages();
                }
            }
        }

        public int TotalItemsOnSelectedPage
        {
            get
            {
                return _itemsOnSelectedPage;
            }

            set
            {
                _itemsOnSelectedPage = value;
                if (_itemsOnSelectedPage < _pageSize)
                {
                    TotalItems = (_selectedPageNumber - 1) * _pageSize + _itemsOnSelectedPage;
                }
            }
        }

        private bool IsFirstPageSelected
        {
            get
            {
                return _selectedPageNumber == 1;
            }
        }

        private bool IsLastPageSelected
        {
            get
            {
                return _selectedPageNumber == TotalPages;
            }
        }

        private void UpdatePages()
        {
            Clear();

            int startPage = GetStartPage();
            for (int pageNumber = startPage; pageNumber <= TotalPages; ++pageNumber)
            {
                if (Count >= _maximumSelectablePages)
                {
                    break;
                }

                Page page = CreatePage(pageNumber);
                Add(page);
            }
        }

        private int GetStartPage()
        {
            // Less pages than can be selected?
            int totalPages = TotalPages;
            if (totalPages <= _maximumSelectablePages)
            {
                return 1;
            }

            // First choice for start page.
            int startPage = _selectedPageNumber - (_maximumSelectablePages / 2);
            if (startPage <= 0)
            {
                return 1;
            }

            // Do we have enough pages?
            int totalPagesBasedOnStartPage = totalPages - startPage + 1;
            if (totalPagesBasedOnStartPage >= _maximumSelectablePages)
            {
                return startPage;
            }

            // Ensure we have enough pages.
            startPage -= _maximumSelectablePages - totalPagesBasedOnStartPage;
            if (startPage > 0)
            {
                return startPage;
            }

            return 1;
        }

        private Page CreatePage(int pageNumber)
        {
            var page = new Page();
            page.Number = pageNumber;
            page.IsSelected = IsSelectedPage(pageNumber);
            return page;
        }

        private bool IsSelectedPage(int pageNumber)
        {
            return pageNumber == _selectedPageNumber;
        }
    }
}