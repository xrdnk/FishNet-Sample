namespace xrdnk.FishNet_Sample.Scripts.Answer
{
    /// <summary>
    /// フルネーム用のドメインオブジェクト
    /// </summary>
    public struct FullName_Answer
    {
        /// <summary>
        /// ファーストネーム（名）
        /// </summary>
        public string FirstName;

        /// <summary>
        /// ラストネーム（姓）
        /// </summary>
        public string LastName;

        /// <summary>
        /// ToString() オーバーライド処理
        /// </summary>
        /// <returns>フルネームで表示</returns>
        public override string ToString()
        {
            return $"{FirstName} {LastName}";
        }
    }
}