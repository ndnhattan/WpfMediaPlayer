   M S B A M L   `   `   `  ����I  EWpfMediaPlayer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null/clr-namespace:WpfMediaPlayerWpfMediaPlayer  5   	       WpfMediaPlayer.MainWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   .localclr-namespace:WpfMediaPlayer     5   	     � Title$  
MainWindow��5   	   $	��450��6   $	��800��6)   .��5	      �  � RowDefinitions 5
   
   �� 5      $��* �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   .+�5   
   r� -   5         r�Name$ mediaPlayer��5       r� LoadedBehavior$ Manual=�6,   5   ;   �� 5   
    �� Orientation$ 
Horizontal=�6   $��0��6.   .+�5      �� $	��150��6   .+�5      �� -   5        $��Add to playlist��5      63   �� 5      -   5        $��Remove Selected��5      63   �� 5      -   5        $��playlistListBox��5       �� SelectionMode$ Extended=�61   $	��130��6J   �� 5      -   5        $
��Save��5      6(   �� 5      -   5        $
��Load��5      6(   �� 5      -   5        $��
ShuffleBtn��5      $��Shuffle mode��6+   6B   �� 5      -   5         ��Name$ recentlyPlayedListBox��5      $	��130��69   5      5      �� 5"   
   -	   5         ��Name$ progressSlider��5"      $��1��6)   $��Center=�5#      $��Center=�6(   $	��400��6E   ${�False��6Q   5$   @   �� 5&   
   $ 
Horizontal=�6   $��2��6.   $��Center=�6;   .+�5'      �� -
   5        $��Previous��5'      6(   �� 5(      -   5        $
��Play��5(      6$   �� 5)      -   5        $��Pause��5)      6%   �� 5*      -   5        $
��Stop��5*      6$   �� 5+      -   5        $
��Next��5+      6$   5-      �� 5/   
   -   5        $ volumeSlider��5/      $��3��6'    � Minimum$ 0Q�64   	 � Maximum$	 1Q�6@   
 � Value$	
 0.5Q�6L   $��Center=�50      $��Center=�6,   $	��200��6I   51      54      55      