// BSD 3-Clause License
//
// Copyright (c) 2020, MainDen
// All rights reserved.
//
// Read more on https://github.com/MainDen/SDK-by-MainDen

namespace MainDen.Collections.Generic
{
    public class LeftRightPair<TLeft, TRight>
    {
        TLeft left;
        TRight right;
        public TLeft Left
        {
            get
            {
                return left;
            }
            set
            {
                left = value;
            }
        }
        public TRight Right
        {
            get
            {
                return right;
            }
            set
            {
                right = value;
            }
        }
        public LeftRightPair(TLeft left, TRight right)
        {
            this.left = left;
            this.right = right;
        }
    }
}