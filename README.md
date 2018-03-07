# FontAwesomeEnum
Creates enums and resources C# classes to be able to use the Font-Awesome icons set (from versions 4 to 5, available here: http://fortawesome.github.io/Font-Awesome/) in .NET a WPF applications.

The generated FontAwesomeEnum.cs file looks like this:

    namespace FontAwesome
    {
      /// <summary>
      /// Font Awesome Resources.
      /// </summary>
      public enum FontAwesomeEnum
      {
        /// <summary>
        /// fa-500px glyph (f26e).
        /// </summary>
        _500px = 0xf26e,

        /// <summary>
        /// fa-address-book glyph (f2b9).
        /// </summary>
        AddressBook = 0xf2b9,

        /// <summary>
        /// fa-address-book-o glyph (f2ba).
        /// </summary>
        AddressBookO = 0xf2ba,

        /// <summary>
        /// fa-address-card glyph (f2bb).
        /// </summary>
        AddressCard = 0xf2bb,
        
        .... etc ...
        
        }
        
        /// <summary>
        /// Font Awesome Resources.
        /// </summary>
        public static partial class FontAwesomeResource
        {
          /// <summary>
          /// fa-500px glyph (f26e).
          /// </summary>
          public const char _500px = '\uf26e';

          /// <summary>
          /// fa-address-book glyph (f2b9).
          /// </summary>
          public const char AddressBook = '\uf2b9';

          /// <summary>
          /// fa-address-book-o glyph (f2ba).
          /// </summary>
          public const char AddressBookO = '\uf2ba';

          /// <summary>
          /// fa-address-card glyph (f2bb).
          /// </summary>
          public const char AddressCard = '\uf2bb';
          
        .... etc ...
        
        }
    }
