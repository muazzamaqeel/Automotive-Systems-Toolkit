   M S B A M L   `   `   `  ����   WpfApp1    MO_TERMINAL.TracingWindow   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Aav2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006       � Title$  Mo-Tools��$	��807��$
��1047�� � WindowStartupLocation$ CenterScreen=� � 
ResizeMode$ 	CanResize=� � 	Resources    PrimaryBackgroundColor �� Color  SecondaryBackgroundColor  AccentColor  	TabsColor  PrimaryTextColor  SecondaryTextColor  SelectedTextColor  StandardButtonStyle �� 
TargetType �� Property �� Value �� 
TargetType	 �� CornerRadius
 �� Triggers P� Property P� Value  CustomRedButtonStyle 	 CustomOrangeButtonStyle 
 SwitchStyle ��Name A�Name �� Triggers �� 
TargetName��%�  &	        &	      &	 &     &	 9     &	 L     &	 _     &	 r     &	 �     7  &	 �    &		 G    &	
 �    �� $ #333333���� $ #444444���� $ #16A085���� $ #000000���� $ #FFFFFF���� $ #CCCCCC���� $ #000000���� " ��.���  � ��8   ��  � ��
 �������  � ��$ 16(���  � ��$
 Bold"���  � ��
 ���� ��  � ��$ 0q���  � ��$ 40����  � ��$	 150����  � �� �� " ��.R��� #��z��#��z��$	 10��$��0q�._��� $��Center=�$��Center=�
 P�  �@J�IsMouseOver$
 True��. ���  � �� �#1ABC9C�� " ��.���  � ��
 �  ����  � ��$ 16(���  � ��$
 Bold"���  � ��
 ���� ��  � ��$ 0q���  � ��$ 40����  � ��$	 150����  � �� �� " ��.R��� #��z��#��z��$	 10��$��0q�._��� $��Center=�$��Center=�
 P�  �@J�IsMouseOver$
 True��. ���  � �� �#1ABC9C�� " ��.���  � ��
 � �����  � ��
 �������  � ��$ 16(���  � ��$
 Bold"���  � ��
 ���� ��  � ��$ 0q���  � ��$ 40����  � ��$	 150����  � �� �� " ��.R��� #��z��#��z��$	 10��$��0q�._��� $��Center=�$��Center=�
 P�  �@J�IsMouseOver$
 True��. ���  � �� �#1ABC9C�� " d�.���  � �� �� " d�.R�� .+��� $ SwitchBackground��#��z��$	 15��$��30��$��70��A� $ 
SwitchKnob��
�������$��26��$��26��$
��Left=�$��2,2,0,2q� P�  �@d�	IsChecked$
 TrueY�. ��� $ 
SwitchKnob�� � ��$ Right=��� $ SwitchBackground�� � ��
 � � �P�  �@d�	IsChecked$ FalseY�. ��� $ 
SwitchKnob�� � ��$
 Left=��� $ SwitchBackground�� � ��
 �  ������  �� 
StartPoint$	 0,0&� �� EndPoint$	 1,1&�.���  � Color$ #2C3E50�� � Offset$ 0Q�� $ #34495E��$ 1Q�.��� #��[  $��0,0,0,5q� � RowDefinitions �� $
��Auto �$��60����  � ColumnDefinitions �� $	��217 ��� .+�]� #��[  $��2��.+��� #��[ $��24(�$
��Bold"�$��10q��� $�CONMOD ���� $� ���� $�Tracing Tool����  �� Orientation$ 
Horizontal=�$��Right=�$��10q�.+��� -   $��Refresh��#��[ $��80���� -   $��Connect��#��[ $��80��$��10,0,0,0q��� -   $��
Disconnect��#��[ $��93��$��10,0,0,0q��� $��1��$��0��$��10,10,10,10q�.+��� $��Select COM Port��#��[ $��0,0,0,5q��� -    ��Name$ COMPortList��#��[ #��[ #��[ $��1q�$��5q�$��14(�$	��115���� $��Select Frame��#��[ $��	10,20,0,5q��� -    ��Name$ FrameSelector��
���   �#��[   �� 	ClickMode d� 	IsChecked �� Path �� Mode �� RelativeSource J� IsHitTestVisible  �� SelectionBoxItem! �� SelectionBoxItemTemplate" �� IsDropDownOpen# � AllowsTransparency$ J� SnapsToDevicePixels% � ActualWidth& �� MaxDropDownHeight' K� IsItemsHost-  'System.Windows.Input.KeyboardNavigation(   DirectionalNavigation��%�  '��       '�� %     7  7  �� " ��.���  � ��
 �   ��� " ��.���  � �� �� " ��.R�� .+�d� $��ToggleButton��$|�False��$ Press=�#��z��#��z�� �� $ IsDropDownOpen�$ TwoWay=� �� *TemplatedParent+.���� $��ContentSite��$ False��#��z  #��z! #��z��$��3,3,23,3q�$��Center=�$
��Left=�� $��Popup��$��Bottom=�#��z" $
# True��$|�False��$
��Fade=�.��� $��DropDown��$
$ True��#��z% #��z& .+��� $��DropDownBorder��8��  8�� $��1q��� $��4,6,4,6q�$
$ True��.���� $
' True��$( 	Contained=� P�  �@��HasItems$ False��. ��� $ DropDownBorder�� � ��$ 95��P�  � {�$ False��. ���  � ��
 �����.@��� $��Frame 1���� $��Frame 2���� $��Frame 3���� $��Frame 4���� $��Select Baud Rate��#��[ $��	10,20,0,5q��� -   $ BaudRateSelector��
���   �#��[  ��%�  '��       '�� %     7  7  �� " ��.���  � ��
 �   ��� " ��.���  � �� �� " ��.R�� .+�d� $��ToggleButton��$|�False��$ Press=�#��z��#��z�� �� $ IsDropDownOpen�$ TwoWay=� �� *TemplatedParent+.���� $��ContentSite��$ False��#��z  #��z! #��z��$��3,3,23,3q�$��Center=�$
��Left=�� $��Popup��$��Bottom=�#��z" $
# True��$|�False��$
��Fade=�.��� $��DropDown��$
$ True��#��z% #��z& .+��� $��DropDownBorder��8��  8�� $��1q��� $��4,6,4,6q�$
$ True��.���� $
' True��$( 	Contained=� P�  �@��HasItems$ False��. ��� $ DropDownBorder�� � ��$ 95��P�  � {�$ False��. ���  � ��
 �����.@��� $
��9600���� $��115200���� -   ) ��Name$) FrameTabControl��$��1��$��1��$��10,10,10,10q�#��[ #��[ #��[ $��1q� * �� Padding+ ��Name��%t  '��       7  7  7  7  7  �� " ��.���  � �� �� " ��.R�� .+��� $��	TabBorder��#��z��8��  $��1q�$	 3,3,0,0��$
* 10,5q�._��� $+ ContentSite��$��Center=�$��Center=�$��Header��$
��True�� P�  �@��
IsSelected$
 True��. ��� $ 	TabBorder��8   � ���� $ 	TabBorder��8   � ���� $ 	TabBorder�� � ��$ 3q��� $ ContentSite�� � ��
 �   �P�  �@��
IsSelected$ False��. ��� $ 	TabBorder��8   � ���� $ ContentSite�� � ��
 �   ��� $ 	TabBorder�� � ��$ 1q�P�  � {�$ False��. ��� $ ContentSite��8   � ��.@��� $��Frame 1��.���  �� $
��101* ��� $
��473* � �� $��* ��� $
��Auto �.+��� -   , ��Name$, Frame1ScrollViewer��$
��Auto=�$��0��$��2��.���� -	   - ��Name$- 
Frame1Data��. �� 
IsReadOnly$
. True��#��[ #��[ #��[ $��1q��� -
   $- Frame1Input��$��10,10,10,10q�#��[ #��[ #��[ $��1q�$��1��$��2���� $��Frame 2��.���  �� $��* ��� $
��Auto �.+��� -   $, Frame2ScrollViewer��$
��Auto=�$��0��.���� -   $- 
Frame2Data��$
. True��#��[ #��[ #��[ $��1q��� -   $- Frame2Input��$��10q�#��[ #��[ #��[ $��1q�$��1���� $��Frame 3��.���  �� $��* ��� $
��Auto �.+��� -   $, Frame3ScrollViewer��$
��Auto=�$��0��.���� -   $- 
Frame3Data��$
. True��#��[ #��[ #��[ $��1q��� -   $- Frame3Input��$��10q�#��[ #��[ #��[ $��1q�$��1���� $��Frame 4��.���  �� $��* ��� $
��Auto �.+��� -   $, Frame4ScrollViewer��$
��Auto=�$��0��.���� -   $- 
Frame4Data��$
. True��#��[ #��[ #��[ $��1q��� -   $- Frame4Input��$��10q�#��[ #��[ #��[ $��1q�$��1��� $��1��$��Center=�$	��Top=�$��	0,310,0,0q�$	��406��$	��198��.+�d� -   / d�Name$/ toggleSwitch��$��70��
�������$��0q�#��[
 $
��Left=�$	��Top=�$��
14,322,0,0q��� #��[ $
��Left=�$��
14,362,0,0q��� $�Toggle Switch���� -   $��K15 ON��#��[ $��108,63,0,224q�$	��NaN���� -   $��K15 Off��#��[ $��108,108,0,179q�$	��NaN���� -   $��WD ON��#��[ $��108,153,0,134q�$	��NaN���� -   $��WD OFF��#��[ $��108,198,0,89q�$	��NaN���� -   $��Password��#��[ $��108,57,2,309q�$	��NaN��0 J� RenderTransformOrigin$0 0.5,0.5&�z�V� .~��� �� 1 �� AngleY$1 -0.228Q��� T� 2 T� Y$2 0.263Q��� -   $��Reset��#��[ $��0,103,99,263q�$	��NaN���� -   $
��BIOS��#��[ $��0,193,99,173q�$	��NaN���� -   $��Marker��#��[ $��0,238,98,128q�$	��NaN���� -   $��Clear��#��[ $��0,148,99,218q�$	��NaN���� -   $��Freeze Trace��#��[	 $��0,58,98,308q�$	��NaN��